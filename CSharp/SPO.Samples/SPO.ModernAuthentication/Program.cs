using Microsoft.Extensions.Configuration;
using SPO.ModernAuthentication.Helpers;
using System;
using System.Security;
using System.Threading.Tasks;

namespace SPO.ModernAuthentication
{
    class Program
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
            Uri site = new Uri(config["Scenario01:SiteUrl"]);
            string clientId = config["Scenario01:ClientId"];
            string user = config["Scenario01:UserName"];
            SecureString password = Utilities.ConvertToSecureString(config["Scenario01:Password"]);

            // Note: The PnP Sites Core AuthenticationManager class also supports this
            using (var authenticationManager = new UserAuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password, clientId))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }

        public static async Task ServicePrincipalAuthTestUsingCertThumbprint(IConfiguration config)
        {
            Console.WriteLine("Testing : ServicePrincipal Auth Test");
            Uri site = new Uri(config["Scenario02:SiteUrl"]);
            string clientId = config["Scenario02:ClientId"];
            string tenantId = config["Scenario02:TenantId"];
            string certtificateThumbprint = config["Scenario02:CertificateThumbprint"];
            var certtificate = CertificateHelper.GetCertificateByThumbprint(certtificateThumbprint);

            using (var authenticationManager = new ServicePrincipalAuthenticationManager())
            using (var context = authenticationManager.GetContext(site, tenantId, clientId, certtificate))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }

        public static async Task ServicePrincipalAuthTestUsingCertPath(IConfiguration config)
        {
            Console.WriteLine("Testing : ServicePrincipal Auth Test");
            Uri site = new Uri(config["Scenario03:SiteUrl"]);
            string clientId = config["Scenario03:ClientId"];
            string tenantId = config["Scenario03:TenantId"];
            string certificatePath = config["Scenario03:CertificatePath"];
            SecureString certificatePassword = Utilities.ConvertToSecureString(config["Scenario03:CertificatePassword"]);            
            var certificate = CertificateHelper.GetCertificateFromPath(certificatePath, certificatePassword);

            // Note: The PnP Sites Core AuthenticationManager class also supports this
            using (var authenticationManager = new ServicePrincipalAuthenticationManager())
            using (var context = authenticationManager.GetContext(site, tenantId, clientId, certificate))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }
    }
}
