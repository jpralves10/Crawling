using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Net.Http.Headers;
using Crawling.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Crawling.Utils;

namespace Crawling.Controllers.Services
{
    public class SiscomexPortalUnico
    {
        public SiscomexPortalUnico() { }

        public SiscomexPortalUnico(X509Certificate2 cert, HttpClientHandler cliHandler, HttpClient httpClient)
        {
            Certificate = cert;
            Handler = cliHandler;
            Client = httpClient;
        }

        public static X509Certificate2 Certificate { get; set; }
        public static HttpClientHandler Handler { get; set; }
        public static HttpClient Client { get; set; }

        public static string setToken { get; set; }
        public static string xCSRFToken { get; set; }
        public static string xCSRFExpiration { get; set; }

        public void CatalogoProdutos(int codigo)
        {
            Client.BaseAddress = new Uri("https://val.portalunico.siscomex.gov.br");

            if (Login().GetAwaiter().GetResult())
            {
                IncluirAlterarProduto();
            }            
        }

        public static async Task<bool> Login()
        {
            try
            {   
                string regexTokens = "(Set-Token:[.\\s\\S]*)";
                string regexCaracteres = "(Set-Token:\\s|X-CSRF-Token:\\s|X-CSRF-Expiration:\\s)";

                var request = new HttpRequestMessage(HttpMethod.Post, "/portal/api/autenticar");

                request.Headers.Add("Role-Type", "IMPEXP");

                var result = await Client.SendAsync(request);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var headers = result.Headers.ToString();

                    Match match_consulta = new Regex(regexTokens).Match(headers);
                    headers = Regex.Replace(match_consulta.Value, regexCaracteres, "");

                    var tokens = headers.Split("\r\n");

                    setToken = tokens[0];
                    xCSRFToken = tokens[1];
                    xCSRFExpiration = tokens[2];

                    var resultJson = result.Content.ReadAsStringAsync();

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERRO");
                return false;
            }
        }

        public static async void IncluirAlterarProduto()
        {
            string url = "https://val.portalunico.siscomex.gov.br/catp/api/ext/produto";

            string produtosJson = catalogoProdutoJson();

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            var content = new StringContent(produtosJson, Encoding.UTF8, "application/json");

            request.Content = content;

            request.Headers.Add("Authorization", setToken);
            request.Headers.Add("X-CSRF-Token", xCSRFToken);

            var response = Client.SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
            }
        }

        public static string catalogoProdutoJson()
        {
            List<CatalogoProduto> listCatalogoProdutos = new List<CatalogoProduto>();
            CatalogoProduto produto = new CatalogoProduto();

            produto.seq = 1;
            produto.codigo = "1";
            produto.descricao = "Produto Teste";
            produto.cnpjRaiz = "00913443";
            produto.situacao = "RASCUNHO";
            produto.modalidade = "IMPORTACAO";
            produto.ncm = "40169990";
            produto.fabricanteConhecido = false;
            produto.paisOrigem = "FR";

            listCatalogoProdutos.Add(produto);

            MongoConnection.CatalogoProduto_doc.InsertManyAsync(listCatalogoProdutos);

            return JsonConvert.SerializeObject(listCatalogoProdutos);
        }
    }
}