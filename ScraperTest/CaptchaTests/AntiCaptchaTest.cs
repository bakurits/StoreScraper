using CheckoutBot.Captcha;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScraperTest.CaptchaTests
{
    [TestClass]
    public class AntiCaptchaTest
    {
        [TestMethod]
        public void TestMethod1Async()
        {
            var captchaHandler = new AntiCaptchaAPI("ca253acc9bd077d1afbb00d422e94a65");
            string siteKey = "6Ldp2bsSAAAAAAJ5uyx_lx34lJeEpTLVkP5k04qc";
            string url = "http://boards.4chan.org/";
            string result = captchaHandler.GetCaptchaResponse(siteKey, url).Result;
            var test = result;
        }
    }
}
