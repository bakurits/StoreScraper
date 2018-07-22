using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.ChampsSports_FootLocker_EastBay;
using StoreScraper.Models;

namespace ScraperTest
{
    [TestClass]
    public class FootScraperTest
    {
        [TestMethod]
        public void ChampsSportsScraper()
        {
            FootStoreScraper footStoreScraper = new FootStoreScraper("ChampsSports", "https://www.champssports.com");
            List<Product> lst = new List<Product>();
            footStoreScraper.FindItems(out lst, null, CancellationToken.None, new Logger());
        }

        [TestMethod]
        public void FootLocker()
        {
            FootStoreScraper footStoreScraper = new FootStoreScraper("FootLocker", "https://www.footlocker.com");
            List<Product> lst = new List<Product>();
            footStoreScraper.FindItems(out lst, null, CancellationToken.None, new Logger());
        }

        [TestMethod]
        public void EastBay()
        {
            FootStoreScraper footStoreScraper = new FootStoreScraper("EastBay", "https://www.eastbay.com");
            List<Product> lst = new List<Product>();
            footStoreScraper.FindItems(out lst, null, CancellationToken.None, new Logger());
        }
    }
}