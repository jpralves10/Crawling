using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crawling.Models
{
    public class SituacaoDespacho
    {
        public Dictionary<string, string> Declaracao { get; set; }
        public Dictionary<string, string> SituacaoDI { get; set; }
        public Dictionary<string, string> DossieDI { get; set; }
        public DocumentosInstrutivos DocumentosInstrutivos { get; set; }
        public string AFRMM { get; set; }
        public string ICMS { get; set; }
        public Dictionary<string, string> SituacaoRVF { get; set; }
    }
}