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
            var prices = new string[] {"500.23 EUR", "USD 1, 300.45" , "300.25 &euro"};

            foreach (var price in prices)
            {
                var parsed = Utils.ParsePrice(price);
                Console.WriteLine(parsed);
            }

            var prices2 = new string[] { "500,23 EUR", "USD 300,45" };

            foreach (var price in prices2)
            {
                var parsed = Utils.ParsePrice(price, decimalDelimiter:",", tousandsDelimiter:"");
                Console.WriteLine(parsed);
            }
        }
    }
}