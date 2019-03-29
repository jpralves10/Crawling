using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Crawling.Models;

namespace Crawling.Utils
{
    public class MongoConnection
    {
        public static MongoClient Client => new MongoClient("mongodb://localhost:27017/crawler");

        public static IMongoDatabase Db => Client.GetDatabase("crawler");

        public static IMongoCollection<CatalogoProduto> CatalogoProduto_doc => Db.GetCollection<CatalogoProduto>("dc_catalogo_produto");


    }
}
