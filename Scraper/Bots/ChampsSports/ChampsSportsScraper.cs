using System;
using System.Collections.Generic;
using System.Threading;
using StoreScraper.Models;

namespace StoreScraper.Bots.ChampsSports
{
    public class ChampsSportsScraper : ScraperBase
    {
        public override string WebsiteName { get; set; }
        public override string WebsiteBaseUrl { get; set; }
        public override Type SearchSettings { get; set; }
        public override bool Enabled { get; set; }

        public override void FindItems(out List<Product> listOfProducts, object settings, CancellationToken token, Logger info)
        {
            throw new NotImplementedException();
        }
    }
}