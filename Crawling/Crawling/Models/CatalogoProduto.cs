using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crawling.Models
{
    public class CatalogoProduto
    {
        public int seq { get; set; }
        //public int codigo { get; set; }
        public string descricao { get; set; }
        public string cnpjRaiz { get; set; }
        public string situacao { get; set; }
        public string modalidade { get; set; }
        public string ncm { get; set; }
        /*public int codigoNaladi { get; set; }
        public int codigoGPC { get; set; }
        public int codigoGPCBrick { get; set; }
        public int codigoUNSPSC { get; set; }*/
        public string paisOrigem { get; set; }
        public Boolean fabricanteConhecido { get; set; }
        /*public string cpfCnpjFabricante { get; set; }
        public string codigoOperadorEstrangeiro { get; set; }
        public List<Atributos> atributos { get; set; }
        public List<string> codigosInterno { get; set; }*/
    }

    public class Atributos
    {
        public string atributo { get; set; }
        public string valor { get; set; }
    }
}


/*{
		  "codigo" : "ATT_1",
		  "nomeApresentacao" : "Destaque",
		  "orientacaoPreenchimento" : "Escolher apenas um Destaque",
		  "formaPreenchimento" : "Lista estática",
		  "modalidade" : "Exportação",
		  "obrigatorio" : true,
		  "dataInicioVigencia" : "2014-10-23",
		  "dominio" : [ {
			"codigo" : "01",
			"descricao" : "EXCETO DE ESPÉCIES DOMÉSTICAS, CONFORME PORTARIA IBAMA 93/98"
		  }, {
			"codigo" : "99",
			"descricao" : "DEMAIS"
		  } ],
		  "objetivos" : [ {
			"codigo" : 3,
			"descricao" : "Tratamento administrativo"
		  } ],
		  "orgaos" : [ "SECEX", "IBAMA" ],
		  "formaPreenchimentoAtributo" : "LISTA_ESTATICA"
		}


[
  {
    "seq": 1,
    "codigo": 123,
    "descricao": "Produto Teste",
    "cnpjRaiz": "00000000",
    "situacao": "ATIVADO",
    "modalidade": "EXPORTACAO",
    "ncm": "02011000",
    "codigoNaladi": 123,
    "codigoGPC": 456,
    "codigoGPCBrick": 789,
    "codigoUNSPSC": 012,
    "paisOrigem": "AR",
    "fabricanteConhecido": true,
    "codigoOperadorEstrangeiro": "123",
    "atributos": [
      {
        "atributo": "ATT_1",
        "valor": "teste"
      }
    ],
    "codigosInterno": [
      "1",
      "2"
    ]
  }
]*/