using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Models;

namespace StoreScraper.Bots.BounceWear
{
    public class BounceWearScrape: ScraperBase
    {
        public override string WebsiteName { get; set; } = "BounceWear";
        public override string WebsiteBaseUrl { get; set; } = "https://bouncewear.com/";
        public override bool Active { get; set; }
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
                
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}