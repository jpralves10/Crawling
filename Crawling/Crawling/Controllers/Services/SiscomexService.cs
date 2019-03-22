using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Crawling.Models;
using System.Net.Http.Headers;
using Crawling.Utils;

namespace Crawling.Controllers.Services
{
    public class SiscomexService
    {

        public SiscomexService() { }

        public SiscomexService(X509Certificate2 cert, HttpClientHandler cliHandler, HttpClient httpClient) {
            Certificate = cert;
            Handler = cliHandler;
            Client = httpClient;
        }

        public static X509Certificate2 Certificate { get; set; }
        public static HttpClientHandler Handler { get; set; }
        public static HttpClient Client { get; set; }

        public void ConsultaLiEmLote(string termo)
        {
            var html = ConsultaLiEmLoteRequest();
        }

        public void SitacaoDespachoAduaneiro(string termo)
        {
            var html = SitacaoDespachoAduaneiroRequest();
            var htmlDocument = new HtmlDocument();

            string text = System.IO.File.ReadAllText(@"C:\Users\Eficilog\Desktop\Eficilog\Crawler\1904306855\RFB - Siscomex Importação Despacho - v1.15 - 25_08_2014 FASE III.html", Encoding.GetEncoding("ISO-8859-1"));

            htmlDocument.LoadHtml(text);
            //htmlDocument.LoadHtml(html.GetAwaiter().GetResult());

            var divs = htmlDocument
                       .DocumentNode.Descendants("div")
                       .Where(node => node.GetAttributeValue("class", "")
                       .Equals("conteudo-frame")).ToList();

            string regexRemocaoTags = "(<style[^>]*>[.\\s\\S]*</style>|<script[^>]*>[.\\s\\S]*</script>|<!--[^-->]*-->|<a[^>]*>|<a([^>]*)>|</a>|<tfoot>[.\\s\\S]*</tfoot>|<b>|</b>|<td[^>]*>[^<^.]+<img[^>]*>[^<^.]+</td>|<img[^>]*>)";

            string regexConsulta1 = "(\\([.\\s\\S]*\\))";
            string regexConsulta2 = "<a href=\"javascript:abrePopupConsultaResumida\\([.\\s\\S]*\\)+;\">Ver consulta resumida</a>";

            string regexSituacaoDI1 = "(\\([.\\s\\S]*\\))";
            string regexSituacaoDI2 = "<a href=\"javascript:openWindow\\([.\\s\\S]*\\)+;\"><img[.\\s\\S]*></a>";

            SituacaoDespacho SituacaoDespacho = new SituacaoDespacho();
            var div_frame = "";

            foreach (var div in divs)
            {
                Match match_consulta = new Regex(regexConsulta1).Match(div.OuterHtml);
                div.InnerHtml = Regex.Replace(div.OuterHtml, regexConsulta2, match_consulta.Value);

                Match match_situacao_di = new Regex(regexSituacaoDI1).Match(div.OuterHtml);
                div.InnerHtml = Regex.Replace(div.OuterHtml, regexSituacaoDI2, match_situacao_di.Value);

                div.InnerHtml = Regex.Replace(div.OuterHtml, regexRemocaoTags, String.Empty);

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

                if (SituacaoDespacho.Declaracao == null)
                {
                    SituacaoDespacho.Declaracao = new Dictionary<string, string>();

                    var trs = div.Descendants("tr").Where(node => node.ParentNode.Name.Equals("tbody")).ToList();

                    foreach (var tr in trs)
                    {
                        var tds = tr.SelectNodes("td").ToList();

                        var declaracao = tds.ToList()[0].InnerHtml.Trim();
                        var situacao = tds.ToList()[1].InnerHtml.Trim();

                        if (!SituacaoDespacho.Declaracao.ContainsKey(declaracao))
                        {
                            SituacaoDespacho.Declaracao.Add(declaracao, situacao);
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
                        var match_motivo = false;

                        foreach (var td in tds)
                        {
                            if (!skipe_td)
                            {
                                Match match_td = new Regex(@":</td>").Match(td.OuterHtml);

                                Match match_td_motivo = new Regex(@"Motivo:</td>").Match(td.OuterHtml);

                                if (match_td.Success)
                                {
                                    keyValue = td.InnerHtml.Trim();
                                    skipe_td = true;

                                    if (match_td_motivo.Success)
                                    {
                                        match_motivo = true;
                                    }
                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                var value_td = td.InnerHtml.Trim();

                                if (match_motivo)
                                {
                                    match_motivo = false;

                                    var param = Regex.Replace(value_td, "(\\(|\\)|&#39;|'|\\s)", String.Empty).Split(",");

                                    var htmlDocumentRVF = new HtmlDocument();

                                    /*string htmlRVF = System.IO.File.ReadAllText(@"C:\Users\Eficilog\Desktop\Eficilog\Crawler\1904306855\com exigência\a exigência\Siscomex Importação Despacho.html", Encoding.GetEncoding("ISO-8859-1"));
                                    htmlDocumentRVF.LoadHtml(htmlRVF);*/

                                    var htmlRVF = ConsultaMotivoRequest(param[0], param[1]);

                                    if (htmlRVF.GetAwaiter().GetResult().Length > 0)
                                    {
                                        htmlDocumentRVF.LoadHtml(htmlRVF.GetAwaiter().GetResult());

                                        var textareas = htmlDocumentRVF.DocumentNode.Descendants("textarea").ToList();

                                        value_td = textareas[0].InnerHtml;
                                    }
                                    else
                                    {
                                        value_td = "";
                                    }
                                }

                                SituacaoDespacho.SituacaoDI.Add(keyValue, value_td);
                                skipe_td = false;
                            }
                        }
                    }
                }

                if (SituacaoDespacho.DossieDI == null && div.Descendants("table").Count() == 2)
                {
                    SituacaoDespacho.DossieDI = new Dictionary<string, string>();

                    var trs = div.Descendants("table").Last().Descendants("tr").ToList();

                    foreach (var tr in trs)
                    {
                        var tds = tr.Descendants("td").ToList();

                        if (tds.Count == 2)
                        {
                            var numero = tds.ToArray()[0].InnerHtml.Trim();
                            var dataHora = tds.ToArray()[1].InnerHtml.Trim();

                            if (!SituacaoDespacho.DossieDI.ContainsKey(numero))
                            {
                                SituacaoDespacho.DossieDI.Add(numero, dataHora);
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

                                if (tds2.Count == 2)
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
                    SituacaoDespacho.SituacaoRVF = new Dictionary<int, Dictionary<string, string>>();

                    var tds_rvf = div.Descendants("td").ToList();

                    var info = tds_rvf.Last().InnerHtml.Trim();

                    var param = Regex.Replace(info, "(\\(|\\)|&#39;|'|\\s)", String.Empty).Split(",");

                    var htmlRVF = ConsultaRVFRequest(param[0], param[1], param[2], param[3], param[4]);

                    if (htmlRVF.GetAwaiter().GetResult().Length > 0)
                    {
                        var htmlDocumentRVF = new HtmlDocument();
                        htmlDocumentRVF.LoadHtml(htmlRVF.GetAwaiter().GetResult());

                        var divRVF = htmlDocumentRVF
                                    .DocumentNode.Descendants("div")
                                    .Where(node => node.GetAttributeValue("class", "")
                                    .Equals("conteudo-frame")).ToList()[0];

                        var table = divRVF.Descendants("table")
                                              .Where(node => node.ParentNode.ParentNode.ParentNode.Name
                                              .Equals("table")).ToList()[0];

                        var trs = table.Descendants("tr").ToList();

                        var ths = table.Descendants("th").ToList();

                        for (int i = 0; i < trs.Count(); i++)
                        {
                            var tds = trs[i].Descendants("td").ToList();

                            if (tds.Count > 0)
                            {
                                var consultaResumida = new Dictionary<string, string>();

                                for (int j = 0; j < ths.Count(); j++)
                                {
                                    consultaResumida.Add(ths.ToArray()[j].InnerHtml.Trim(),
                                                         tds.ToArray()[j].InnerHtml.Trim());
                                }
                                SituacaoDespacho.SituacaoRVF.Add(i, consultaResumida);
                            }
                            continue;
                        }
                    }
                }
            }

            var SituacaoDespachoJson = JsonConvert.SerializeObject(SituacaoDespacho);
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
            string url = @"https://www1c.siscomex.receita.fazenda.gov.br/impdespacho-web-7/ConsultarDetalheResumidoPopup.do?" +
                "numero=" + numero +
                "&sequencial=" + sequencial +
                "&situacao=" + situacao +
                "&quantidade=" + quantidade +
                "&tipoRVF=" + tiporvf;

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

        private static async Task<string> ConsultaMotivoRequest(string numero, string tipoInterrupcao)
        {
            string url = @"https://www1c.siscomex.receita.fazenda.gov.br/impdespacho-web-7/AcompanharSituacaoDespachoMotivoInterrupcao.do?" +
                "numero=" + numero +
                "&tipoInterrupcao=" + tipoInterrupcao;

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

        private static async Task<string> ConsultaLiEmLoteRequest()
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
    }
}