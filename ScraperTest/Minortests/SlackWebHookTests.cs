﻿using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Bakurits.Mrporter;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace ScraperTest.MinorTests
{
    [TestClass()]
    public class SlackWebHookTests
    {
        [TestMethod()]
        public void PostMessageTest()
        {
            Product product = new Product(new MrporterScraper(), "JOHN ELLIOTT Camp-Collar Printed Tencel-Twill Shirt",
                "https://www.mrporter.com/mens/okeeffe/bristol-leather-trimmed-suede-derby-shoes/1026175",
                120.83,
                "https://cache.mrporter.com/images/products/1012326/1012326_mrp_in_l.jpg",
                "id");
            var task = SlackWebHook.PostMessage(product,
                "https://hooks.slack.com/services/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");


            try
            {
                task.Result.EnsureSuccessStatusCode();
            }
            finally
            {
                Debug.WriteLine(task.Result.Content.ReadAsStringAsync().Result);
            }

            Thread.Sleep(5000);

        }
    }
}