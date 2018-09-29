using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Shopify
{
    [DisableInGUI]
    public abstract class ShopifyScraper : ScraperBase
    {     
        public virtual List<string> JsonEndpoints { get; set; } = new List<string>();
        public virtual List<string> XmlSitemapEndpoints { get; set; } = new List<string>();
        public virtual ShopifyScrapMode DefaultScrapMode { get; set; } = ShopifyScrapMode.XmlSitemap;


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            throw new NotSupportedException();
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            throw  new NotSupportedException();
        }

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var client = ClientFactory.GetProxiedFirefoxClient(null, true);


            switch (DefaultScrapMode)
            {
                case ShopifyScrapMode.JsonProductList:
                    break;
                case ShopifyScrapMode.XmlSitemap:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //var rootModel = JsonConvert.DeserializeObject<JsonProductsRoot>();
        }



        private void ScraperJsonProducts()
        {

        }

    }


    public enum ShopifyScrapMode
    {
        JsonProductList,
        XmlSitemap,
    }
}
