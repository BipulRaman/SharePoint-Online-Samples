// <copyright file="ServicePrincipalAuthenticationManager.cs" company="Bipul Raman">
// Copyright (c) Bipul Raman. All rights reserved.
// </copyright>

namespace SPO.ModernAuthentication.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;
    using Microsoft.SharePoint.Client;

    /// <summary>
    /// ServicePrincipalAuthenticationManager Class.
    /// </summary>
    public class ServicePrincipalAuthenticationManager : IDisposable
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        // Token cache handling
        private static readonly SemaphoreSlim SemaphoreSlimTokens = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<string, string> tokenCache = new ConcurrentDictionary<string, string>();
        private AutoResetEvent tokenResetEvent = null;
        private bool disposedValue;

        /// <summary>
        /// Method to Get SPO Client Context.
        /// </summary>
        /// <param name="web">SPO Web Url.</param>
        /// <param name="tenantId">SPO Tenant ID.</param>
        /// <param name="clientId">AAD App Client ID.</param>
        /// <param name="clientCertificate">AAD Client Certificate.</param>
        /// <returns>SPO Client Context.</returns>
        public ClientContext GetContext(Uri web, string tenantId, string clientId, X509Certificate2 clientCertificate)
        {
            var context = new ClientContext(web);

            context.ExecutingWebRequest += (sender, e) =>
            {
                string accessToken = this.EnsureAccessTokenAsync(new Uri($"{web.Scheme}://{web.DnsSafeHost}"), tenantId, clientId, clientCertificate).GetAwaiter().GetResult();
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
            };

            return context;
        }

        /// <summary>
        /// Method to Ensure Access Token Async.
        /// </summary>
        /// <param name="resourceUri">Resource Uri.</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="clientId">Client Id.</param>
        /// <param name="clientCertificate">Client Certificate.</param>
        /// <returns>Access Token.</returns>
        public async Task<string> EnsureAccessTokenAsync(Uri resourceUri, string tenantId, string clientId, X509Certificate2 clientCertificate)
        {
            string accessTokenFromCache = TokenFromCache(resourceUri, this.tokenCache);
            if (accessTokenFromCache == null)
            {
                await SemaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                try
                {
                    // No async methods are allowed in a lock section
                    string accessToken = await this.AcquireTokenAsync(resourceUri, tenantId, clientId, clientCertificate).ConfigureAwait(false);
                    Console.WriteLine($"Successfully requested new access token resource {resourceUri.DnsSafeHost} for user {clientId}");
                    AddTokenToCache(resourceUri, this.tokenCache, accessToken);

                    // Register a thread to invalidate the access token once's it's expired
                    this.tokenResetEvent = new AutoResetEvent(false);
                    TokenWaitInfo wi = new TokenWaitInfo();
                    wi.Handle = ThreadPool.RegisterWaitForSingleObject(
                        this.tokenResetEvent,
                        async (state, timedOut) =>
                        {
                            if (!timedOut)
                            {
                                TokenWaitInfo internalWaitToken = (TokenWaitInfo)state;
                                if (internalWaitToken.Handle != null)
                                {
                                    internalWaitToken.Handle.Unregister(null);
                                }
                            }
                            else
                            {
                                try
                                {
                                    // Take a lock to ensure no other threads are updating the SharePoint Access token at this time
                                    await SemaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                                    RemoveTokenFromCache(resourceUri, this.tokenCache);
                                    Console.WriteLine($"Cached token for resource {resourceUri.DnsSafeHost} and user {clientId} expired");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Something went wrong during cache token invalidation: {ex.Message}");
                                    RemoveTokenFromCache(resourceUri, this.tokenCache);
                                }
                                finally
                                {
                                    SemaphoreSlimTokens.Release();
                                }
                            }
                        },
                        wi,
                        (uint)CalculateThreadSleep(accessToken).TotalMilliseconds,
                        true);

                    return accessToken;
                }
                finally
                {
                    SemaphoreSlimTokens.Release();
                }
            }
            else
            {
                Console.WriteLine($"Returning token from cache for resource {resourceUri.DnsSafeHost} and user {clientId}");
                return accessTokenFromCache;
            }
        }

        /// <summary>
        /// Dispose Method.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose Method.
        /// </summary>
        /// <param name="disposing">Disposing Value.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.tokenResetEvent != null)
                    {
                        this.tokenResetEvent.Set();
                        this.tokenResetEvent.Dispose();
                    }
                }

                this.disposedValue = true;
            }
        }

        private static string TokenFromCache(Uri web, ConcurrentDictionary<string, string> tokenCache)
        {
            if (tokenCache.TryGetValue(web.DnsSafeHost, out string accessToken))
            {
                return accessToken;
            }

            return null;
        }

        private static void AddTokenToCache(Uri web, ConcurrentDictionary<string, string> tokenCache, string newAccessToken)
        {
            if (tokenCache.TryGetValue(web.DnsSafeHost, out string currentAccessToken))
            {
                tokenCache.TryUpdate(web.DnsSafeHost, newAccessToken, currentAccessToken);
            }
            else
            {
                tokenCache.TryAdd(web.DnsSafeHost, newAccessToken);
            }
        }

        private static void RemoveTokenFromCache(Uri web, ConcurrentDictionary<string, string> tokenCache)
        {
            tokenCache.TryRemove(web.DnsSafeHost, out string currentAccessToken);
        }

        private static TimeSpan CalculateThreadSleep(string accessToken)
        {
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(accessToken);
            var lease = GetAccessTokenLease(token.ValidTo);
            lease = TimeSpan.FromSeconds(lease.TotalSeconds - TimeSpan.FromMinutes(5).TotalSeconds > 0 ? lease.TotalSeconds - TimeSpan.FromMinutes(5).TotalSeconds : lease.TotalSeconds);
            return lease;
        }

        private static TimeSpan GetAccessTokenLease(DateTime expiresOn)
        {
            DateTime now = DateTime.UtcNow;
            DateTime expires = expiresOn.Kind == DateTimeKind.Utc ? expiresOn : TimeZoneInfo.ConvertTimeToUtc(expiresOn);
            TimeSpan lease = expires - now;
            return lease;
        }

        private async Task<string> AcquireTokenAsync(Uri resourceUri, string tenantId, string clientId, X509Certificate2 clientCertificate)
        {
            string[] scopes = new string[] { $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}/.default" };
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithTenantId(tenantId)
            .WithCertificate(clientCertificate)
            .Build();

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }

        /// <summary>
        /// TokenWaitInfo Internal Class.
        /// </summary>
        internal class TokenWaitInfo
        {
            /// <summary>
            /// RegisteredWaitHandle.
            /// </summary>
            public RegisteredWaitHandle Handle = null;
        }
    }
}
