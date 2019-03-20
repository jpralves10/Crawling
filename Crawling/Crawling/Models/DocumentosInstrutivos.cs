using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crawling.Models
{
    public class DocumentosInstrutivos
    {
        public string DataHora { get; set; }
        public string Matricula { get; set; }
        public string Nome { get; set; }
        public Dictionary<string, string> Documentos { get; set; }
    }
}
