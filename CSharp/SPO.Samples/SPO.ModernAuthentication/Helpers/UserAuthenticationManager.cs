// <copyright file="UserAuthenticationManager.cs" company="Bipul Raman">
// Copyright (c) Bipul Raman. All rights reserved.
// </copyright>

namespace SPO.ModernAuthentication.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Http;
    using System.Security;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.SharePoint.Client;

    /// <summary>
    /// UserAuthenticationManager Class.
    /// </summary>
    public class UserAuthenticationManager : IDisposable
    {

        private const string TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";
        private static readonly HttpClient HttpClient = new HttpClient();

        // Token cache handling
        private static readonly SemaphoreSlim SemaphoreSlimTokens = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<string, string> tokenCache = new ConcurrentDictionary<string, string>();
        private AutoResetEvent tokenResetEvent = null;

        private bool disposedValue;

        /// <summary>
        /// Method to get SPO Context.
        /// </summary>
        /// <param name="web">SPO Web Url.</param>
        /// <param name="userPrincipalName">UserName.</param>
        /// <param name="userPassword">Password.</param>
        /// <param name="clientId">AAD App Client Id.</param>
        /// <returns>SPO Client Context.</returns>
        public ClientContext GetContext(Uri web, string userPrincipalName, SecureString userPassword, string clientId)
        {
            var context = new ClientContext(web);

            context.ExecutingWebRequest += (sender, e) =>
            {
                string accessToken = this.EnsureAccessTokenAsync(new Uri($"{web.Scheme}://{web.DnsSafeHost}"), userPrincipalName, new System.Net.NetworkCredential(string.Empty, userPassword).Password, clientId).GetAwaiter().GetResult();
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
            };

            return context;
        }

        /// <summary>
        /// Method to Ensure Access Token Async.
        /// </summary>
        /// <param name="resourceUri">Resource Uri.</param>
        /// <param name="userPrincipalName">UPN.</param>
        /// <param name="userPassword">UserName.</param>
        /// <param name="clientId">AAD App Client Id.</param>
        /// <returns>Access Token.</returns>
        public async Task<string> EnsureAccessTokenAsync(Uri resourceUri, string userPrincipalName, string userPassword, string clientId)
        {
            string accessTokenFromCache = TokenFromCache(resourceUri, this.tokenCache);
            if (accessTokenFromCache == null)
            {
                await SemaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                try
                {
                    // No async methods are allowed in a lock section
                    string accessToken = await this.AcquireTokenAsync(resourceUri, userPrincipalName, userPassword, clientId).ConfigureAwait(false);
                    Console.WriteLine($"Successfully requested new access token resource {resourceUri.DnsSafeHost} for user {userPrincipalName}");
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
                                    Console.WriteLine($"Cached token for resource {resourceUri.DnsSafeHost} and user {userPrincipalName} expired");
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
                Console.WriteLine($"Returning token from cache for resource {resourceUri.DnsSafeHost} and user {userPrincipalName}");
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
        /// <param name="disposing">Disposing.</param>
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

        private async Task<string> AcquireTokenAsync(Uri resourceUri, string username, string password, string clientId)
        {
            string resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";

            var body = $"resource={resource}&client_id={clientId}&grant_type=password&username={HttpUtility.UrlEncode(username)}&password={HttpUtility.UrlEncode(password)}";
            using (var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {
                var result = await HttpClient.PostAsync(TokenEndpoint, stringContent).ContinueWith((response) =>
                {
                    return response.Result.Content.ReadAsStringAsync().Result;
                }).ConfigureAwait(false);

                var tokenResult = JsonSerializer.Deserialize<JsonElement>(result);
                var token = tokenResult.GetProperty("access_token").GetString();
                return token;
            }
        }

        /// <summary>
        /// TokenWaitInfo Class.
        /// </summary>
        internal class TokenWaitInfo
        {
            public RegisteredWaitHandle Handle = null;
        }
    }
}
