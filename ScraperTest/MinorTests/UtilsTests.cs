using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Helpers;

namespace ScraperTest.MinorTests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void ParsePriceTest()
        {
            var prices = new string[] {"500.23 EUR"};

            foreach (var price in prices)
            {
                var parsed = Utils.ParsePrice(price);
                Console.WriteLine(parsed.Value);
                Console.WriteLine(parsed.Currency);
            }
        }
    }
}