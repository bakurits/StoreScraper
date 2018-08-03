using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Core;
using StoreScraper.Models;

namespace ScraperTest.Models
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void WriteLogTest()
        {
            Logger.Instance.WriteVerboseLog("bakuri");
        }
    }
}