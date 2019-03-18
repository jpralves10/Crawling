using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Crawling.Utils
{
    public static class CertificateManager
    {

        public static X509Certificate2 CurrentCertificate { get; set; }

        public static X509Certificate2 GetCertificate(string thumbprint)
        {
            var certificates = GetAllCertificates();

            return certificates.FirstOrDefault(c => c.Thumbprint == thumbprint);
        }

        private static X509Certificate2Collection GetAllCertificates(StoreLocation tipo)
        {
            X509Store store = new X509Store(StoreName.My, tipo);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates;
        }

        public static X509Certificate2Collection GetAllUserCertificates()
        {
            return GetAllCertificates(StoreLocation.CurrentUser);
        }

        public static List<X509Certificate2> GetAllCertificates()
        {
            var returnList = new List<X509Certificate2>();

            var machineCerticates = GetAllCertificates(StoreLocation.LocalMachine);
            var userCertificates = GetAllCertificates(StoreLocation.CurrentUser);

            foreach (var cert in machineCerticates)
            {
                returnList.Add(cert);
            }

            foreach (var cert in userCertificates)
            {
                returnList.Add(cert);
            }

            return returnList;
        }

        public static bool IsValidCertificate(X509Certificate2 certificate)
        {
            try
            {
                var handler = new HttpClientHandler();

                handler.ClientCertificates.Add(certificate);

                var httpClient = new HttpClient(handler);

                return httpClient.GetAsync("https://www1c.siscomex.receita.fazenda.gov.br/siscomexImpweb-7/private_siscomeximpweb_inicio.do").Result.StatusCode == HttpStatusCode.OK;
            } catch
            {
                return false;
            }
        }
    }
}
