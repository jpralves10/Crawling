using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

using HtmlAgilityPack;
using System.Net.Http;
using System.Net;
using System.Web;
using System.IO;
using Newtonsoft.Json;

using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace CrawlingDesktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            //await CrawlerTask(textSearch.Text);
            await CrawlerApiTask(textSearch.Text);

            textSearch.Text = "";   
        }

        private void listSearched_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            listSearched.Items.Clear();
        }

        private async Task CrawlerTask(string termo)
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
            string Id, Titulo, Link, Subtitulo;

            foreach (var div in divs)
            {
                countId++;
                Id = countId.ToString();
                Titulo = div.Descendants("h2").FirstOrDefault().InnerText;
                Link = div.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value;
                Subtitulo = div.Descendants("div")
                                .Where(node => node.GetAttributeValue("class", "")
                                .Equals("price_last_modele"))
                                .FirstOrDefault().InnerText;

                ListViewItem listView = new ListViewItem(Id);
                listView.SubItems.Add(Titulo);
                listView.SubItems.Add(Link);
                listView.SubItems.Add(Subtitulo);

                listSearched.Items.Add(listView);
            }
        }

        private async Task CrawlerApiTask(string searchQuery)
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
            string Titulo, Link, Subtitulo;

            foreach (var item in jsonData.items)
            {
                countId++;

                Titulo = item.title;
                Link = item.link;
                Subtitulo = item.snippet;

                ListViewItem listView = new ListViewItem(countId.ToString());
                listView.SubItems.Add(Titulo);
                listView.SubItems.Add(Link);
                listView.SubItems.Add(Subtitulo);

                listSearched.Items.Add(listView);
            }
        }
    }
}