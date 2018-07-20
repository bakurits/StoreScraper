using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Bots.ChampsSports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Models;

namespace ScraperTest
{
    [TestClass]
    public class ChampsSportsTest
    {
        [TestMethod]
        public void test1()
        {
            ChampsSportsScraper champsSportsScraper = new ChampsSportsScraper();
            List<Product> lst = new List<Product>();
            champsSportsScraper.FindItems(out lst, null, CancellationToken.None, new Logger());
        }
    }
}