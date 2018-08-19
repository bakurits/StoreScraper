using System.Net;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Core;

namespace ScraperTest.MinorTests
{
    [TestClass]
    public class LoggerTest
    {
        
        [TestMethod]
        public void TestSnapshotSave()
        {
            Logger logger = new Logger();
            string html = new WebClient().DownloadString("http://www.google.com/");
            var document = new HtmlDocument();
            document.LoadHtml(html);
            logger.SaveHtmlSnapshop(document);
        }
    }
}
