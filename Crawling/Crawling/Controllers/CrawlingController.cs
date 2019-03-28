using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crawling.Models;
using Crawling.Utils;
using Crawling.Controllers.Services;

namespace Crawling.Controllers
{
    public class CrawlingController : Controller
    {
        public static List<CrawlingModel> list = new List<CrawlingModel>();
        public static string termoConsulta = "";

        public static X509Certificate2 Certificate { get; set; }
        public static HttpClientHandler Handler { get; set; }
        public static HttpClient Client { get; set; }
        
        public IActionResult Index()
        {
            ViewData["Message"] = termoConsulta;

            return View(list);
        }

        public IActionResult Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            if(list.Count > 0)
            {
                list.RemoveAll(r => r.Id == id);
            }

            return RedirectToAction("Index", "Crawling");
        }

        [HttpPost]
        public async Task<IActionResult> PesquisarAsync(string termoPesquisa)
        {
            list = new List<CrawlingModel>();


            if (termoPesquisa == null || termoPesquisa.Length <= 0)
            {
                termoConsulta = $"Term not searched !";
            }
            else
            {
                //await CrawlerTask(termoPesquisa);
                //await CrawlerApiTask(termoPesquisa);

                setParametrosConexcao();

                AcessarServicos(termoPesquisa);

                if (list.Count > 0)
                {
                    termoConsulta = $"Term '{termoPesquisa}' searched with success !";
                }
                else
                {
                    termoConsulta = $"Term '{termoPesquisa}' not searched !";
                }
            }

            return RedirectToAction("Index", "Crawling");
        }

        public static void setParametrosConexcao()
        {
            Certificate = CertificateManager.GetAllCertificates().FirstOrDefault(c => c.FriendlyName.Contains("WAGNER"));

            Handler = new HttpClientHandler();
            Handler.ClientCertificates.Add(Certificate);
            Handler.CookieContainer = new CookieContainer();
            Handler.UseCookies = true;

            Client = new HttpClient(Handler);
        }

        public static void AcessarServicos(string termo)
        {
            /*var siscomex = new SiscomexReceita(Certificate, Handler, Client);
            //siscomex.SitacaoDespachoAduaneiro(termo);
            siscomex.ConsultaLiEmLote(termo);*/

            var siscomex = new SiscomexPortalUnico(Certificate, Handler, Client);
            siscomex.CatalogoProdutos(10);
        }

        /*private static async Task CrawlerTask(string termo)
        {
            var url = "https://www.automobile.tn/neuf/bmw.3/";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var divs = htmlDocument
                       .DocumentNode.Descendants("div")
                       .Where(node => node.GetAttributeValue("class", "")
                       .Equals("article_new_car article_last_modele")).ToList();

            int countId = 0;

            foreach (var div in divs)
            {
                countId++;
                var crawler = new CrawlingModel
                {
                    Id = countId,
                    Titulo = div.Descendants("h2").FirstOrDefault().InnerText,
                    Link = div.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value,
                    Subtitulo = div.Descendants("div")
                                   .Where(node => node.GetAttributeValue("class", "")
                                   .Equals("price_last_modele"))
                                   .FirstOrDefault().InnerText
                };
                list.Add(crawler);
            }
        }*/

        /*private static async Task CrawlerApiTask(string searchQuery)
        {
            searchQuery = searchQuery.Replace(' ', '-');

            string cx = "013209936556916087702:wysbzarp5x4";
            //string apiKey = "AIzaSyA_Q8Y7XlwqzVm-HPmCC3mihugudmvJTx0";
            string apiKey = "AIzaSyAt-vzuhcbmR82w-UJcm-3gUyWucxwHjSI";
            var request = WebRequest.Create("https://www.googleapis.com/customsearch/v1?key=" + apiKey + "&cx=" + cx + "&q=" + searchQuery);

            WebResponse response = await request.GetResponseAsync();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseString = reader.ReadToEnd();
            dynamic jsonData = JsonConvert.DeserializeObject(responseString);
            
            int countId = 0;

            if(jsonData.items != null)
            {
                foreach (var item in jsonData.items)
                {
                    countId++;
                    var crawler = new CrawlingModel
                    {
                        Id = countId,
                        Titulo = item.title,
                        Link = item.link,
                        Subtitulo = item.snippet
                    };
                    list.Add(crawler);
                }
            }            
        }*/
    }
}