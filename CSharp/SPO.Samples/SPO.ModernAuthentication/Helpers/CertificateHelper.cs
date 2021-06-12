using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ModernAuthentication.Helpers
{
    public static class CertificateHelper
    {
        public static X509Certificate2 GetCertificateByThumbprint(string thumbPrint)
        {
            X509Certificate2 appOnlyCertificate = null;
            using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);
                if (certCollection.Count > 0)
                {
                    appOnlyCertificate = certCollection[0];
                }
                certStore.Close();
                return appOnlyCertificate;
            }
        }
        public static X509Certificate2 GetCertificateFromPath(string certPath, SecureString certPassword)
        {
            X509Certificate2 appOnlyCertificate = new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.MachineKeySet);
            return appOnlyCertificate;
        }
    }
}