using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crawling.Models
{
    public class CatalogoProduto
    {
        [BsonId()]
        public int seq { get; set; }

        [BsonElement("codigo")]
        public string codigo { get; set; }

        [BsonElement("descricao")]
        public string descricao { get; set; }

        [BsonRequired()]
        [BsonElement("cnpjRaiz")]
        public string cnpjRaiz { get; set; }

        [BsonRequired()]
        [BsonElement("situacao")]
        public string situacao { get; set; }

        [BsonRequired()]
        [BsonElement("modalidade")]
        public string modalidade { get; set; }

        [BsonRequired()]
        [BsonElement("ncm")]
        public string ncm { get; set; }

        [BsonElement("codigoNaladi")]
        public Nullable<int> codigoNaladi { get; set; }

        [BsonElement("codigoGPC")]
        public Nullable<int> codigoGPC { get; set; }

        [BsonElement("codigoGPCBrick")]
        public Nullable<int> codigoGPCBrick { get; set; }

        [BsonElement("codigoUNSPSC")]
        public Nullable<int> codigoUNSPSC { get; set; }

        [BsonRequired()]
        [BsonElement("paisOrigem")]
        public string paisOrigem { get; set; }

        [BsonRequired()]
        [BsonElement("fabricanteConhecido")]
        public Boolean fabricanteConhecido { get; set; }

        [BsonElement("cpfCnpjFabricante")]
        public string cpfCnpjFabricante { get; set; }

        [BsonElement("codigoOperadorEstrangeiro")]
        public string codigoOperadorEstrangeiro { get; set; }

        [BsonElement("atributos")]
        public List<Atributos> atributos { get; set; }

        [BsonElement("codigosInterno")]
        public List<string> codigosInterno { get; set; }
    }

    public class Atributos
    {
        [BsonElement("atributo")]
        public string atributo { get; set; }

        [BsonElement("valor")]
        public string valor { get; set; }
    }
}