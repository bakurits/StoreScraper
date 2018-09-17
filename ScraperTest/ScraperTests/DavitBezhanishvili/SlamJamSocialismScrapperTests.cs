using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Jordan.Ruvilla;
using StoreScraper.Bots.Jordan.SlamJamSocialism;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.DavitBezhanishvili
{
    [TestClass]
    public class SlamJamSocialismScrapperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            var scraper = new SlamJamSocialismScraper();
            var settings = new SearchSettingsBase()
            {
                KeyWords = "shoes"
            };

            scraper.FindItems(out var list, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(list);
        }

        [TestMethod]
        public void NewArrivalsTest()
        {
            var scraper = new SlamJamSocialismScraper();

            scraper.ScrapeNewArrivalsPage(out var list, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(list);
        }


        [TestMethod]
        public void GetProductDetailsTest()
        {
            
            var scraper = new SlamJamSocialismScraper();

            var testUrl = "https://www.slamjamsocialism.com/coats/53736-long-coat.html";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Helper.PrintGetDetailsResult(details.SizesList);
        }
    }
}
