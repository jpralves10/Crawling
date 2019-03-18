using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crawling.Models;
using System.Net.Http.Headers;
using Crawling.Utils;

namespace Crawling.Controllers
{
    public class CrawlingController : Controller
    {
        public static List<CrawlingModel> list = new List<CrawlingModel>();
        public static string termoConsulta = "";

        public static X509Certificate2 Certificate { get; set; }
        public static HttpClientHandler Handler { get; set; }
        public static HttpClient Client { get; set; }

        /*public CrawlingController()
        {
            list.Add(new CrawlingModel { Id = 1, Titulo = "Google", Link = "www.google.com", Subtitulo = "Teste de Crawling" });
        }*/

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

                if (Login().GetAwaiter().GetResult())
                {
                    await SitacaoDespachoAduaneiro(termoPesquisa);
                }

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
            Client.BaseAddress = new Uri("https://www1c.siscomex.receita.fazenda.gov.br");
        }
        
        public static async Task<bool> Login()
        {
            try
            {
                var status = await Client.GetAsync("/siscomexImpweb-7/private_siscomeximpweb_inicio.do");
                return status.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERRO");
                return false;
            }
        }

        private static async Task SitacaoDespachoAduaneiro (string termo)
        {
            var url = "https://www1c.siscomex.receita.fazenda.gov.br/impdespacho-web-7/AcompanharSituacaoDespacho.do";

            Handler = new HttpClientHandler();
            Handler.ClientCertificates.Add(Certificate);
            Handler.CookieContainer = new CookieContainer();
            Handler.UseCookies = true;

            Client = new HttpClient(Handler);
            Client.BaseAddress = new Uri("https://www1c.siscomex.receita.fazenda.gov.br");

            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("fase", "a"));
            parameters.Add(new KeyValuePair<string, string>("nrDeclaracao", ""));
            parameters.Add(new KeyValuePair<string, string>("declaracoesArray", "19/0430685-5"));

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(parameters) };

            request.Headers.Add("Accept", "ext/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Host = "www1c.siscomex.receita.fazenda.gov.br";
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            request.Headers.Connection.Add("keep-alive");
            request.Headers.Referrer = new Uri("https://www1c.siscomex.receita.fazenda.gov.br/impdespacho-web-7/AcompanharSituacaoDespachoMenu.do");

            var html = (await Client.SendAsync(request)).Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html.GetAwaiter().GetResult());

            var divs = htmlDocument
                       .DocumentNode.Descendants("div")
                       .Where(node => node.GetAttributeValue("class", "")
                       .Equals("conteudo-frame")).ToList();
            
            string regex = "(<style[^>]*>[.\\s\\S]*</style>|<script[^>]*>[.\\s\\S]*</script>|<!--[^-->]*-->|<a[^>^openWindow]*>|</a>|<tfoot>[.\\s\\S]*</tfoot>|<b>|</b>|<td[^>]*>[^<^.]+<img[^>]*>[^<^.]+</td>|<img[^>]*>)";
            
            string regexLink = "[^>]*<a[^<]*</a>";


            //string output = JsonConvert.SerializeObject(product);
                                  


            Dictionary<string, string> chavesValores = new Dictionary<string, string>();

            foreach (var div in divs)
            {
                div.InnerHtml = Regex.Replace(div.OuterHtml, regex, String.Empty);
                div.InnerHtml = Regex.Replace(div.OuterHtml, regexLink, "[openWindow]");

                var ths = div.Descendants("th").Where(node => node.ParentNode.ParentNode.Name.Equals("thead")).ToList();

                if (ths.Count() > 0)
                {
                    var trs = div.Descendants("tr").Where(node => node.ParentNode.Name.Equals("tbody")).ToList();

                    foreach (var tr in trs)
                    {
                        var tds = tr.Descendants("td").ToList();

                        if (tds.Count() == ths.Count())
                        {
                            for (int i = 0; i < ths.Count(); i++)
                            {
                                //var teste2 = ths.ToArray()[i].InnerHtml + tds.ToArray()[i].InnerHtml;

                                chavesValores.Add(ths.ToArray()[i].InnerHtml.Trim(), tds.ToArray()[i].InnerHtml.Trim());
                            }
                        }
                    }
                }
                else
                {
                    var trs = div.Descendants("tr").Where(node => node.ParentNode.Name.Equals("table")).ToList();

                    foreach (var tr in trs)
                    {
                        var tds = tr.Descendants("td").ToList();
                        
                        Regex regex_td = new Regex(@":</td>");
                        Match match_td = regex_td.Match(tds.ToArray()[0].OuterHtml);

                        if (!match_td.Success)
                        {
                            tds.ToArray()[0].Remove();
                        }                       

                        foreach (var td in tds)
                        {
                           
                            
                        }
                    }
                }


            }                                

 
            
            /*int countId = 0;

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
            }*/
        }

        private static async Task CrawlerTask(string termo)
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
        }

        private static async Task CrawlerApiTask(string searchQuery)
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
        }
    }
}