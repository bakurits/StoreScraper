using System.Threading;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScraperTest.CheckoutBots.FootSites
{
    [TestClass]
    public class checkReleaseTimeTest
    {
        [TestMethod]
        public void check()
        {
            CheckRelaeseDates checker = new CheckRelaeseDates();
            checker.checkRelaese();
        }

        [TestMethod]
        public void testCheckPost()
        {
            CheckRelaeseDates checker = new CheckRelaeseDates();
            FootsitesProduct product = new FootsitesProduct(null, "sd", "https://www.eastbay.com/product/model:303959/sku:00500617/", 0, "", "", "", null);
            checker.checkPost(product, CancellationToken.None);
        }
    }
}
