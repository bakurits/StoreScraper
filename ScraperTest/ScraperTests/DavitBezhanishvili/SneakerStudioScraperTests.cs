using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.DavitBezhanishvili.SneakerStudioScraper;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.DavitBezhanishvili
{
    [TestClass]
    public class SneakerStudioScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            var scraper = new SneakerStudioScraper();
            var settings = new SearchSettingsBase()
            {
                KeyWords = "adidas"
            };

            scraper.FindItems(out var list,settings, CancellationToken.None);
            Helper.PrintFindItemsResults(list);
        }
        [TestMethod]
        public void GetProductDetailsTest1()
        {
            var scraper = new SneakerStudioScraper();

            var testUrl = "https://sneakerstudio.com/product-eng-16693-Scarf-Fila-686015-006.html";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Helper.PrintGetDetailsResult(details.SizesList);
        }
        [TestMethod]
        public void GetProductDetailsTest2()
        {
            var scraper = new SneakerStudioScraper();

            var testUrl = "https://sneakerstudio.com/product-eng-14715-Mens-shoes-sneakers-adidas-Originals-Padiham-SPZL-Spezial-Night-Navy-AC7747.html";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Helper.PrintGetDetailsResult(details.SizesList);
        }[TestMethod]

        public void GetProductDetailsTest3()
        {
            var scraper = new SneakerStudioScraper();

            var testUrl = "https://sneakerstudio.com/product-eng-15474-Womens-shoes-sneakers-New-Balance-WL574ESS.html";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Helper.PrintGetDetailsResult(details.SizesList);
        }
    
    }
}
