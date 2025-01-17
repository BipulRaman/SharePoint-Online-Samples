﻿using Microsoft.Extensions.Configuration;
using SPO.ModernAuthentication.Helpers;
using System.Security;
using System.Threading.Tasks;
using PnP.Framework;
using System;

namespace SPO.ModernAuth
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Scenario 1 : By User Authentication for Deligated Access
            await DelegatedUserAuthTest(config);

            // Scenario 2 : By Service Principal Authentication for Application Access using Cert Thumprint
            await ServicePrincipalAuthTestUsingCertThumbprint(config);

            // Scenario 3 : By Service Principal Authentication for Application Access using Cert Path
            await ServicePrincipalAuthTestUsingCertPath(config);  
        }

        public static async Task DelegatedUserAuthTest(IConfiguration config)
        {
            Console.WriteLine("Testing : Delegated User Auth Test");
            string siteUrl = config["Scenario01:SiteUrl"];
            string clientId = config["Scenario01:ClientId"];
            string userName = config["Scenario01:UserName"];
            SecureString password = Utilities.ConvertToSecureString(config["Scenario01:Password"]);

            var authManager = new AuthenticationManager(clientId, userName, password);
            using (var context = authManager.GetContext(siteUrl))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }

        public static async Task ServicePrincipalAuthTestUsingCertThumbprint(IConfiguration config)
        {
            Console.WriteLine("Testing : ServicePrincipal Auth Test");
            string siteUrl = config["Scenario01:SiteUrl"];
            string clientId = config["Scenario02:ClientId"];
            string tenantId = config["Scenario02:TenantId"];
            string certtificateThumbprint = config["Scenario02:CertificateThumbprint"];
            var certificate = CertificateHelper.GetCertificateByThumbprint(certtificateThumbprint);

            var authManager = new AuthenticationManager(clientId, certificate, tenantId);
            using (var context = authManager.GetContext(siteUrl))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }

        public static async Task ServicePrincipalAuthTestUsingCertPath(IConfiguration config)
        {
            Console.WriteLine("Testing : ServicePrincipal Auth Test");
            string siteUrl = config["Scenario01:SiteUrl"];
            string clientId = config["Scenario03:ClientId"];
            string tenantId = config["Scenario03:TenantId"];
            string certificatePath = config["Scenario03:CertificatePath"];
            string certificatePassword = config["Scenario03:CertificatePassword"];

            var authManager = new AuthenticationManager(clientId, certificatePath, certificatePassword, tenantId);

            using (var context = authManager.GetContext(siteUrl))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }
    }
}
