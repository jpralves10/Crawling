using System;
using System.Collections.Generic;
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

namespace Crawling.Controllers
{
    public class CrawlingController : Controller
    {
        static List<CrawlingModel> list = new List<CrawlingModel>();
        static string termoConsulta = "";

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
            //await CrawlerTask(termoPesquisa);
            await CrawlerApiTask(termoPesquisa);

            if (list.Count > 0)
            {
                termoConsulta = $"Term '{termoPesquisa}' searched with success !";
            }
            
            return RedirectToAction("Index", "Crawling");
        }

        // *** OBS.: Crawling para Google não funciona, somente via API ***

        /*private static async Task CrawlerTask(string termo)
        {
            var url = "https://www.google.com.br/search?q=" + termo + "&oq=" + termo;
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var divs = htmlDocument
                       .DocumentNode.Descendants("div")
                       .Where(node => node.GetAttributeValue("class", "")
                       .Equals("rc")).ToList();

            int countId = 0;

            foreach(var div in divs)
            {
                countId++;
                var crawler = new CrawlingModel
                {
                    Id = countId,
                    Titulo = div.Descendants("h3").FirstOrDefault().InnerText,
                    Link = div.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value,
                    Subtitulo = div.Descendants("span").FirstOrDefault().InnerText
                };
                list.Add(crawler);
            }
        }*/

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
            string cx = "009702158656880571322:jjsb8vtiq0y";
            string apiKey = "AIzaSyA_Q8Y7XlwqzVm-HPmCC3mihugudmvJTx0";
            var request = WebRequest.Create("https://www.googleapis.com/customsearch/v1?key=" + apiKey + "&cx=" + cx + "&q=" + searchQuery);

            WebResponse response = await request.GetResponseAsync();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseString = reader.ReadToEnd();
            dynamic jsonData = JsonConvert.DeserializeObject(responseString);
            
            int countId = 0;

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