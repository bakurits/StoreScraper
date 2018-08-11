using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Bakurits.Shelflife;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace ScraperTest.MinorTests
{
    [TestClass]
    public class DiscordWebhookTests
    {
        //[TestMethod]
        public void SendTest()
        {
            Product product = new Product(new ShelflifeScraper(), "Vans x Van Gogh UA SK8-Hi - Almond Blossom/True White",
                "https://www.shelflife.co.za/products/Vans-x-Van-Gogh-UA-SK8-Hi-Almond-Blossom-True-White",
                1399.00,
                "https://www.shelflife.co.za/pics/product/large/vn0a38geubl-side.jpg",
                "id", "£");
            var task = DiscordWebhook.Send(
                "https://discordapp.com/api/webhooks/468240680414609429/kKJB9L4I8AfQWWDcqf0vpAj9OYDqxLAJ9gHl1b2B5xg8c5X2Ic4FpcSHAE8_0vKqZBoP",
                product);

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