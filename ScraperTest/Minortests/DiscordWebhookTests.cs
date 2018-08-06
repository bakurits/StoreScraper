using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Mrporter;
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
            Product product = new Product(new MrporterScraper(), "JOHN ELLIOTT Camp-Collar Printed Tencel-Twill Shirt",
                "https://www.mrporter.com/mens/okeeffe/bristol-leather-trimmed-suede-derby-shoes/1026175",
                120.83,
                "https://cache.mrporter.com/images/products/1069726/1069726_mrp_in_m2.jpg",
                "id");
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