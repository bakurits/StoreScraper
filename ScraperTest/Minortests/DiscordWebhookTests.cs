using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Mrporter;
using StoreScraper.Bots.Shelflife;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace ScraperTest.Minortests
{
    [TestClass()]
    public class DiscordWebhookTests
    {
        [TestMethod()]
        public void SendTest()
        {
            Product product = new Product(new ShelflifeScraper(), "Vans x Van Gogh UA SK8-Hi - Almond Blossom/True White",
                "https://www.shelflife.co.za/products/Vans-x-Van-Gogh-UA-SK8-Hi-Almond-Blossom-True-White",
                1399.00,
                "https://www.shelflife.co.za/pics/product/large/vn0a38geubl-side.jpg",
                "id", "R");
            DiscordWebhook.Send(
                "https://discordapp.com/api/webhooks/468240680414609429/kKJB9L4I8AfQWWDcqf0vpAj9OYDqxLAJ9gHl1b2B5xg8c5X2Ic4FpcSHAE8_0vKqZBoP",
                product).Wait();
            Thread.Sleep(5000);
        }
    }
}