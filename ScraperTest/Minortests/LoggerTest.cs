using System;
using System.Net;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Core;
using StoreScraper.Http;

namespace ScraperTest.Minortests
{
    [TestClass]
    public class LoggerTest
    {
        
        [TestMethod]
        public void TestSnapshotSave()
        {
            Logger logger = new Logger();
            string html = new WebClient().DownloadString("https://www.google.com/");
            var document = new HtmlDocument();
            document.LoadHtml(html);
            logger.SaveHtmlSnapshop(document);
        }
    }
}
