using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Bakurits.Mrporter;
using StoreScraper.Bots.Bakurits.Shelflife;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace ScraperTest.MinorTests
{
    [TestClass()]
    public class SlackWebHookTests
    {
        [TestMethod()]
        public void PostMessageTest()
        {
            ProductDetails productDetails = new ProductDetails
            {
                ScrapedBy = new ShelflifeScraper(),
                Name = "Vans x Van Gogh UA SK8-Hi - Almond Blossom/True White",
                Url = "http://www.shelflife.co.za/products/Vans-x-Van-Gogh-UA-SK8-Hi-Almond-Blossom-True-White",
                Price = 1399.00,
                ImageUrl = "http://www.shelflife.co.za/pics/product/large/vn0a38geubl-side.jpg",
                Id = "http://www.shelflife.co.za/products/Vans-x-Van-Gogh-UA-SK8-Hi-Almond-Blossom-True-White",
                Currency = "£",
                SizesList = new List<StringPair>
                {
                    new StringPair() {Key = "L", Value = "1"},
                    new StringPair() {Key = "XL", Value = "1"},
                    new StringPair() {Key = "XXL", Value = "1"}
                }
            };
            var task = new SlackPoster().PostMessage("http://hooks.slack.com/services/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", productDetails, CancellationToken.None);


            try
            {
                task.Result.EnsureSuccessStatusCode();
            }
            finally
            {
                Debug.WriteLine(task.Result.Content.ReadAsStringAsync().Result);
            }
        }
    }
}