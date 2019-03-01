using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crawling.Models;

namespace Crawling.Controllers
{
    public class CrawlingController : Controller
    {
        static List<CrawlingModel> list = new List<CrawlingModel>();

        public CrawlingController()
        {
            list.Add(new CrawlingModel { Id = 1, Titulo = "Google", Link = "www.google.com", Subtitulo = "Teste de Crawling" });
        }

        public IActionResult Index()
        {
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
    }
}