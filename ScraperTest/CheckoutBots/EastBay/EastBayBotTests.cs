using System.Threading;
using CheckoutBot.CheckoutBots.EastBay;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScraperTest.CheckoutBots.EastBay
{
    [TestClass()]
    public class EastBayBotTests
    {
        [TestMethod()]
        public void GuestCheckOutTest()
        {

        }

        [TestMethod()]
        public void AccountCheckoutTest()
        {

        }

        [TestMethod()]
        public void ScrapeReleasePageTest()
        {
            EastBayBot bot = new EastBayBot();
            bot.ScrapeReleasePage(CancellationToken.None);
        }
    }
}