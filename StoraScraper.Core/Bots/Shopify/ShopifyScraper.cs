using System;
using System.Collections.Generic;
using System.Threading;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Models;

namespace StoreScraper.Bots.Shopify
{
    [DisableInGUI]
    public abstract class ShopifyScraper : ScraperBase
    {     
        public virtual List<string> JsonEndpoints { get; set; } = new List<string>();
        public virtual List<string> XmlSitemapEndpoints { get; set; } = new List<string>();



        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            throw new NotSupportedException();
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
