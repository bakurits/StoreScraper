using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Shelflife;
using StoreScraper.Models;

namespace ScraperTest.Bots.Shelflife
{
    [TestClass()]
    public class ShelflifeScraperTests
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            ShelflifeScraper scraper = new ShelflifeScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "watch"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            foreach (var item in lst)
            {
                Debug.WriteLine(item.Name);
                Debug.WriteLine(item.Url);
                Debug.WriteLine(item.ImageUrl);
                Debug.WriteLine(item.Price);
                Debug.WriteLine("");
                Debug.WriteLine("");
            }
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            throw new NotImplementedException();
        }
    }
}