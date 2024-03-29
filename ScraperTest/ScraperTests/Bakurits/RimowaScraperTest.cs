﻿using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Bakurits.Rimowa;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass]
    public class RimowaScraperTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            RimowaScraper scraper = new RimowaScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "luggage"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new RimowaScraper(), "INCOTEX Slim - Fit Pleated Brushed Stretch - Cotton Trousers",
                "https://www.rimowa.com/de/en/variation?pid=83273&dwvar_83273_color=red_gloss",
                2399,
                "pics/product/large/aj2998-200-side.jpg",
                "id");


            RimowaScraper scraper = new RimowaScraper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);

        }
        [TestMethod]
        public void GetNewArrivalsPage()
        {
            RimowaScraper scraper = new RimowaScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
    
}
