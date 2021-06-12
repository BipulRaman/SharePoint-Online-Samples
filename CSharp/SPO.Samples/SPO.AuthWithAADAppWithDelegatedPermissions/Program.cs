using System;
using System.Security;
using System.Threading.Tasks;
using SPO.AuthWithAADAppWithDelegatedPermissions.Helpers;

namespace SPO.AuthWithAADAppWithDelegatedPermissions
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Uri site = new Uri("https://m365x706493.sharepoint.com/sites/TeamSite01");
            string user = "admin@M365x706493.onmicrosoft.com";
            SecureString password = ConvertToSecureString("");

            // Note: The PnP Sites Core AuthenticationManager class also supports this
            using (var authenticationManager = new AuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }

        private static SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("Password is NULL.");

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
    }
}
