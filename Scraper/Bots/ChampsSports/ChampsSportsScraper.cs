using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckOutBot.Interfaces;
using CheckOutBot.Models;

namespace CheckOutBot.Bots.ChampsSports
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
