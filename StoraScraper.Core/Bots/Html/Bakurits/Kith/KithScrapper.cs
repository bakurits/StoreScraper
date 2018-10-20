using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

#pragma warning disable 4014

namespace StoreScraper.Bots.Html.Bakurits.Kith
{
    public class KithScrapper : ScraperBase
    {
        
        public override string WebsiteName { get; set; } = "Kith";
        public override string WebsiteBaseUrl { get; set; } = "https://kith.com/";
        public override bool Active { get; set; }
       
        
        private static readonly string[] Urls = { "https://kith.com/collections/latest",
                                                  "https://kith.com/collections/womens-latest-products",
                                                  "https://kith.com/collections/latest-kids"
        };
        

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            ConcurrentDictionary<Product, byte> products = new ConcurrentDictionary<Product, byte>();
            var pages = Utils.GetPageTask(Urls.ToList(), token).Result;
            foreach (var page in pages)
            {
                var items = page.SelectNodes("//a[@class='product-card-info']");
                foreach (var item in items)
                {
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    LoadSingleProduct(products, null, item);
#else
                    LoadSingleProductTryCatchWrapper(listOfProducts, null, item);
#endif
                }
            }
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void LoadSingleProductTryCatchWrapper(ConcurrentDictionary<Product, byte>  listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, item);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private void LoadSingleProduct(ConcurrentDictionary<Product, byte>  listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            
        }
    }
}