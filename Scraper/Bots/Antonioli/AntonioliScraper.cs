using System;
using System.Collections.Generic;
using System.Threading;
using StoreScraper.Models;

namespace StoreScraper.Bots.Antonioli
{
    public class AntonioliScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Antonioli";
        public override string WebsiteBaseUrl { get; set; } = "https://www.antonioli.eu";
        public override bool Active { get; set; }

        public override Type SearchSettings { get; set; } = typeof(AntonioliSearchSettingsBase);


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}