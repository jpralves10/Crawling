using System;
using System.Text;
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

        private static async Task SitacaoDespachoAduaneiro(string termo)
        {
            var html = SitacaoDespachoAduaneiroRequest();
            var htmlDocument = new HtmlDocument();
            
            string text = System.IO.File.ReadAllText(@"C:\Users\Eficilog\Desktop\Eficilog\Crawler\1904306855\RFB - Siscomex Importação Despacho - v1.15 - 25_08_2014.html", Encoding.GetEncoding("ISO-8859-1"));

            htmlDocument.LoadHtml(text);
            //htmlDocument.LoadHtml(html.GetAwaiter().GetResult());

            var divs = htmlDocument
                       .DocumentNode.Descendants("div")
                       .Where(node => node.GetAttributeValue("class", "")
                       .Equals("conteudo-frame")).ToList();

            string regexRemocaoTags = "(<style[^>]*>[.\\s\\S]*</style>|<script[^>]*>[.\\s\\S]*</script>|<!--[^-->]*-->|<a[^>]*>|<a([^>]*?:[openWindow]*)>|</a>|<tfoot>[.\\s\\S]*</tfoot>|<b>|</b>|<td[^>]*>[^<^.]+<img[^>]*>[^<^.]+</td>|<img[^>]*>)";
            string regexLink = "openWindow";
            string regexConsulta1 = "(\\([.\\s\\S]*\\))";
            string regexConsulta2 = "<a href=\"javascript:abrePopupConsultaResumida\\([.\\s\\S]*\\)+;\">Ver consulta resumida</a>";

            //string output = JsonConvert.SerializeObject(product);

            SituacaoDespacho SituacaoDespacho = new SituacaoDespacho();
            var div_frame = "";

            foreach (var div in divs)
            {
                Match match_consulta = new Regex(regexConsulta1).Match(div.OuterHtml);

                div.InnerHtml = Regex.Replace(div.OuterHtml, regexConsulta2, match_consulta.Value);
                div.InnerHtml = Regex.Replace(div.OuterHtml, regexRemocaoTags, String.Empty);
                div.InnerHtml = Regex.Replace(div.OuterHtml, regexLink, "[openWindow]");

                try
                {
                    div_frame = div.Descendants("div")
                                   .Where(node => node.GetAttributeValue("class", "")
                                   .Equals("titulo-frame")).ToList().FirstOrDefault().InnerHtml;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Teste");
                }

                Match match_situacaoDI = new Regex(@"Situação DI").Match(div_frame);
                Match match_situacaoRVF = new Regex(@"Situação do RVF").Match(div_frame);
                Match match_documentos = new Regex(@"Recepção de documentos").Match(div_frame);
                Match match_afrmm = new Regex(@"AFRMM").Match(div_frame);
                Match match_icms = new Regex(@"ICMS").Match(div_frame);

                var ths = div.Descendants("th").Where(node => node.ParentNode.ParentNode.Name.Equals("thead")).ToList();

                if (SituacaoDespacho.Declaracao == null)
                {
                    if (ths.Count() > 0)
                    {
                        SituacaoDespacho.Declaracao = new Dictionary<string, string>();

                        var trs = div.Descendants("tr").Where(node => node.ParentNode.Name.Equals("tbody")).ToList();

                        foreach (var tr in trs)
                        {
                            var tds = tr.SelectNodes("td").ToList();

                            if (tds.Count() == ths.Count())
                            {
                                for (int i = 0; i < ths.Count(); i++)
                                {
                                    SituacaoDespacho.Declaracao.Add(ths.ToArray()[i].InnerHtml.Trim(), 
                                                                    tds.ToArray()[i].InnerHtml.Trim());
                                }
                            }
                        }
                    }
                }

                if (SituacaoDespacho.SituacaoDI == null && match_situacaoDI.Success)
                {
                    SituacaoDespacho.SituacaoDI = new Dictionary<string, string>();
                    
                    var trs = div.Descendants("tr").Where(node => node.ParentNode.Name.Equals("tbody")).ToList();

                    foreach (var tr in trs)
                    {
                        var tds = tr.Descendants("td").ToList();

                        var keyValue = "";
                        var skipe_td = false;

                        foreach (var td in tds)
                        {
                            if (!skipe_td)
                            {
                                Regex regex_td = new Regex(@":</td>");
                                Match match_td = regex_td.Match(td.OuterHtml);

                                if (match_td.Success)
                                {
                                    keyValue = td.InnerHtml.Trim();
                                    skipe_td = true;
                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                SituacaoDespacho.SituacaoDI.Add(keyValue, td.InnerHtml.Trim());
                                skipe_td = false;
                            }
                        }
                    }
                }

                if (SituacaoDespacho.DossieDI == null && div.Descendants("table").Count() == 2)
                {
                    SituacaoDespacho.DossieDI = new Dictionary<string, string>();

                    var ths2 = div.Descendants("th").Where(node => node.ParentNode.ParentNode.Name.Equals("tbody")).ToList();

                    if (ths2.Count() > 0)
                    {
                        var trs = div.Descendants("tr").Where(node => node.ParentNode.Name.Equals("tbody")).ToList();

                        foreach (var tr in trs)
                        {
                            var tds = tr.Descendants("td").ToList();

                            if (tds.Count() == ths2.Count())
                            {
                                for (int i = 0; i < ths2.Count(); i++)
                                {
                                    SituacaoDespacho.DossieDI.Add(ths2.ToArray()[i].InnerHtml.Trim(), 
                                                                  tds.ToArray()[i].InnerHtml.Trim());
                                }
                            }
                        }
                    }
                }

                if (SituacaoDespacho.DocumentosInstrutivos == null && match_documentos.Success)
                {
                    SituacaoDespacho.DocumentosInstrutivos = new DocumentosInstrutivos();
                    SituacaoDespacho.DocumentosInstrutivos.Documentos = new Dictionary<string, string>();

                    var trs = div.Descendants("tr").ToList();

                    SituacaoDespacho.DocumentosInstrutivos.DataHora = trs[0].Descendants("td").ToList()[1].InnerHtml.Trim();
                    SituacaoDespacho.DocumentosInstrutivos.Matricula = trs[1].Descendants("td").ToList()[1].InnerHtml.Trim();
                    SituacaoDespacho.DocumentosInstrutivos.Nome = trs[2].Descendants("td").ToList()[1].InnerHtml.Trim();

                    foreach (var tr in trs)
                    {
                        var tables2 = tr.Descendants("table").ToList();

                        if (tables2.Count > 0)
                        {
                            var trs_table2 = tr.Descendants("tr").ToList();

                            foreach (var tr2 in trs_table2)
                            {
                                var th_table2 = tr2.Descendants("th").ToList();

                                if (th_table2.Count() > 0)
                                {
                                    continue;
                                }

                                var tds2 = tr2.Descendants("td").ToList();
                                    
                                if(tds2.Count == 2)
                                {
                                    string numero = tds2.ToArray()[1].InnerHtml.Trim();
                                    string documento = tds2.ToArray()[0].InnerHtml.Trim();

                                    if (!SituacaoDespacho.DocumentosInstrutivos.Documentos.ContainsKey(numero))
                                    {
                                        SituacaoDespacho.DocumentosInstrutivos.Documentos.Add(numero, documento);
                                    }
                                }
                            }
                        }
                    }
                }

                if (SituacaoDespacho.AFRMM == null && match_afrmm.Success)
                {
                    var td_afrmm = div.Descendants("td").ToList()[0];

                    SituacaoDespacho.AFRMM = td_afrmm.InnerHtml.Trim();
                }

                if (SituacaoDespacho.ICMS == null && match_icms.Success)
                {
                    var td_icms = div.Descendants("td").ToList()[0];

                    SituacaoDespacho.ICMS = td_icms.InnerHtml.Trim();
                }

                if (SituacaoDespacho.SituacaoRVF == null && match_situacaoRVF.Success)
                {
                    SituacaoDespacho.SituacaoRVF = new Dictionary<string, string>();

                    var tds_rvf = div.Descendants("td")
                                     .Where(node => node.ParentNode.ParentNode.Name.Equals("table")).ToList();

                    var info = tds_rvf.Last().InnerHtml.Trim();

                    Func<int, int> func = x => x - 2;

                    var param = info.Substring(1, func(info.Length)).Split(",");

                    var htmlRVF = ConsultaRVFRequest(param[0].Substring(1, func(param[0].Length)),
                                                     param[1].Trim(),
                                                     param[2].Trim(),
                                                     param[3].Trim(),
                                                     param[4].Trim());

                    var htmlDocumentRVF = new HtmlDocument();
                    htmlDocumentRVF.LoadHtml(htmlRVF.GetAwaiter().GetResult());

                    var divRVF = htmlDocumentRVF
                                .DocumentNode.Descendants("div")
                                .Where(node => node.GetAttributeValue("class", "")
                                .Equals("conteudo-frame")).ToList()[0];

                    var table_rvf = divRVF.Descendants("table")
                                          .Where(node => node.ParentNode.ParentNode.ParentNode.Name
                                          .Equals("table")).ToList()[0];

                    var ths_rvf_consulta = table_rvf.Descendants("th").ToList();
                    var tds_rvf_consulta = table_rvf.Descendants("td").ToList();

                    if (tds_rvf_consulta.Count() == ths_rvf_consulta.Count())
                    {
                        for (int i = 0; i < ths.Count(); i++)
                        {
                            SituacaoDespacho.SituacaoRVF.Add(ths_rvf_consulta.ToArray()[i].InnerHtml.Trim(), 
                                                             tds_rvf_consulta.ToArray()[i].InnerHtml.Trim());
                        }
                    }
                }
            }

            var teste = SituacaoDespacho;
        }

        private static async Task<string> SitacaoDespachoAduaneiroRequest()
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

            var response = (await Client.SendAsync(request)).Content.ReadAsStringAsync();

            return await response;
        }

        private static async Task<string> ConsultaRVFRequest(string numero, string sequencial, string situacao, string quantidade, string tiporvf)
        {
            //var url = "https://www1c.siscomex.receita.fazenda.gov.br/impdespacho-web-7/ConsultarDetalheResumidoPopup.do";

            string url = @"https://www1c.siscomex.receita.fazenda.gov.br/impdespacho-web-7/ConsultarDetalheResumidoPopup.do?" + 
                "numero=" + numero + 
                "&sequencial=" + sequencial + 
                "&situacao=" + situacao + 
                "&quantidade=" + quantidade + 
                "&tipoRVF=" + tiporvf;

            //Uri uri = new Uri(url);
            
            Handler = new HttpClientHandler();
            Handler.ClientCertificates.Add(Certificate);
            Handler.CookieContainer = new CookieContainer();
            Handler.UseCookies = true;

            Client = new HttpClient(Handler);
            Client.BaseAddress = new Uri("https://www1c.siscomex.receita.fazenda.gov.br");

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("Accept", "ext/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Host = "www1c.siscomex.receita.fazenda.gov.br";
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            request.Headers.Connection.Add("keep-alive");
            request.Headers.Referrer = new Uri("https://www1c.siscomex.receita.fazenda.gov.br/impdespacho-web-7/AcompanharSituacaoDespacho.do");

            var response = (await Client.SendAsync(request)).Content.ReadAsStringAsync();

            return await response;
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