using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper;
using StoreScraper.Bots.Mrporter;
using StoreScraper.Models;

namespace ScraperTest
{
    [TestClass]
    public class Mrporter
    {
        [TestMethod]
        public void TestMethod1()
        {
            MrporterScraper scraper = new MrporterScraper();
            MrporterSearchSettings settings = new MrporterSearchSettings()
            {
                KeyWords = "bag"
            };
           
            scraper.FindItems(out var lst, settings, CancellationToken.None, new Logger());
            foreach (var item in lst)
            {
                Debug.WriteLine(item);
            }
            
        }

        [TestMethod]
        public void TestMethod2()
        {
            Product curProduct = new Product("mrporter", "JOHN ELLIOTT Camp-Collar Printed Tencel-Twill Shirt",
                "https://www.mrporter.com/mens/okeeffe/bristol-leather-trimmed-suede-derby-shoes/1026175",
                120.83,
                "id",
                "https://cache.mrporter.com/images/products/1012326/1012326_mrp_in_l.jpg");


            MrporterScraper scraper = new MrporterScraper();

            scraper.GetProductDetails(curProduct, CancellationToken.None);
        }

        [TestMethod]
        public void TestMethod3()
        {
            Product curProduct = new Product("mrporter", "SACAI Slim - Fit Bleached Black Watch Checked Linen Trousers",
                "https://www.mrporter.com/mens/sacai/slim-fit-bleached-black-watch-checked-linen-trousers/1007487?ppv=2",
                120.83,
                "id",
                "https://cache.mrporter.com/images/products/1012326/1012326_mrp_in_l.jpg");


            MrporterScraper scraper = new MrporterScraper();

            scraper.GetProductDetails(curProduct, CancellationToken.None);
        }
    }
}
