using Microsoft.VisualStudio.TestTools.UnitTesting;
using CheckoutBot.TwoCaptcha;

namespace ScraperTest.CaptchaTests
{
    [TestClass]
    public class TwoCaptchaTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var captchaHandler = new TwoCaptchaAPI("7db9f83d5c1d875c786258690bc4264f");
            string siteKey = "6Ldp2bsSAAAAAAJ5uyx_lx34lJeEpTLVkP5k04qc";
            string url = "http://boards.4chan.org/";
            string response;
            //
        }
    }
}
