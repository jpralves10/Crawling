using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Net.Http.Headers;
using Crawling.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using System;

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

        public void CatalogoProdutos(int codigo)
        {
            Client.BaseAddress = new Uri("https://portalunico.siscomex.gov.br/portal");

            Login();
            IncluirAlterarProduto();
        }

        public static async Task<bool> Login()
        {
            try
            {
                var status = await Client.GetAsync("/#/");
                return status.StatusCode == HttpStatusCode.OK;
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

            string listaProdutos = catalogoProdutoJson();

            //string listaProdutos = "[{ \"seq\":1, \"descricao\":\"Produto Teste\", \"cnpjRaiz\":\"0289824000158\", \"situacao\":\"ATIVADO\", \"modalidade\":\"IMPORTACAO\", \"ncm\":\"01019000\", \"paisOrigem\":\"AR\", \"fabricanteConhecido\":false }]";

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = Client.PostAsJsonAsync(url, listaProdutos).Result;

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
            produto.descricao = "Produto Teste";
            produto.cnpjRaiz = "0289824000158";
            produto.situacao = "RASCUNHO";
            produto.modalidade = "IMPORTACAO";
            produto.ncm = "01019000";
            produto.fabricanteConhecido = false;
            produto.paisOrigem = "AR";

            listCatalogoProdutos.Add(produto);

            return JsonConvert.SerializeObject(listCatalogoProdutos);
        }
    }
}