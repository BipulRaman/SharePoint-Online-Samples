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
            // Scenario 1 : By User Authentication for Deligated Access
            await DelegatedUserAuthTest();

            // Scenario 2 : By Service Principal Authentication for Application Access
            await ServicePrincipalAuthTest();
        }

        public static async Task DelegatedUserAuthTest()
        {
            Console.WriteLine("Testing : Delegated User Auth Test");
            Uri site = new Uri("https://m365x706493.sharepoint.com/sites/TeamSite01");
            string clientId = "31359c7f-bd7e-475c-86db-fdb8c937548e";
            string user = "admin@M365x706493.onmicrosoft.com";
            SecureString password = Utilities.ConvertToSecureString("");

            // Note: The PnP Sites Core AuthenticationManager class also supports this
            using (var authenticationManager = new UserAuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password, clientId))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }

        public static async Task ServicePrincipalAuthTest()
        {
            Console.WriteLine("Testing : ServicePrincipal Auth Test");
            Uri site = new Uri("https://m365x706493.sharepoint.com/sites/TeamSite01");
            string clientId = "3a13e474-c67f-4ee4-a1ba-a62d42f1934e";
            string tenantId = "6d0ecd4e-9354-4baf-a69c-2242a179a5d3";

            // Option -1 : Using Cert Thumbprint
            Console.WriteLine("Testing : Using Certificate Thumbprint");
            string certThumbprint = "E8FDE3DDC4C8167F8D99D4F1E3FC312FB3193ECD";
            var cert1 = CertificateHelper.GetCertificateByThumbprint(certThumbprint);

            using (var authenticationManager = new ServicePrincipalAuthenticationManager())
            using (var context = authenticationManager.GetContext(site, tenantId, clientId, cert1))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }

            // Option - 2 : Using Certificate PFX file and it's password.
            Console.WriteLine("Testing : Using Certificate PFX File and Password");
            SecureString certPassword = Utilities.ConvertToSecureString("");
            var cert2 = CertificateHelper.GetCertificateFromPath("D:\\SPODemo02.pfx", certPassword);

            // Note: The PnP Sites Core AuthenticationManager class also supports this
            using (var authenticationManager = new ServicePrincipalAuthenticationManager())
            using (var context = authenticationManager.GetContext(site, tenantId, clientId, cert2))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }
    }
}
