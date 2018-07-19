using System;
using System.Collections.Generic;
using System.Threading;
using Flurl.Http;
using HtmlAgilityPack;
using StoreScraper.Browser;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mrporter
{
    public class MrporterScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Mrporter";
        public override string WebsiteBaseUrl { get; set; } = "https://www.mrporter.com/";
        public override Type SearchSettings { get; set; } = typeof(MrporterSearchSettings);
        public override bool Enabled { get; set; }


        private const string SearchUrlFormat = @"https://www.mrporter.com/en-ge/mens/search/{0}?keywords={0}&pn={1}";


        public override void FindItems(out List<Product> listOfProducts, object settingsObj, CancellationToken token, Logger info)
        {

            var settings = (MrporterSearchSettings)settingsObj;
            listOfProducts = new List<Product>();

            var node = GetPage(settings.KeyWords, 1, token);
            string pageCountHtml = node.SelectSingleNode("//span[@class='page-on']").InnerHtml;

            int pageCount = Convert.ToInt32(pageCountHtml.Substring(pageCountHtml.LastIndexOf(";", StringComparison.Ordinal) + 1));

            for (int page = 1; page <= pageCount; page++)
            {
                Worker(listOfProducts, settings.KeyWords, page, token);
            }

        }


        private HtmlNode GetPage(string keywords, int page, CancellationToken token)
        {
            var searchUrl = string.Format(SearchUrlFormat, keywords, page);
            var request = searchUrl.WithProxy().WithHeaders(ClientFactory.ChromeHeaders);
            var document = request.GetDoc(token);
            return document.DocumentNode;
        }


        private void Worker(List<Product> listOfProducts, string keywords, int page, CancellationToken token)
        {
            var node = GetPage(keywords, page, token);
            HtmlNodeCollection collection =
                node.SelectNodes(
                    "//div[@class='product-details']/div[@class='description']|//div[@class='product-details']/div[@class='description description-last']");

            foreach (var htmlNode in collection)
            {

                string url = htmlNode.SelectSingleNode("./div/a").GetAttributeValue("href", null);
                string name = htmlNode.SelectSingleNode("./div/a").GetAttributeValue("title", null);

                var priceContainer = htmlNode.SelectSingleNode("./div[@class='price-container']");

                var newPrice = priceContainer.SelectSingleNode("./span[@class='price-value']");
                double price = 0;
                if (newPrice != null)
                {
                    price = Convert.ToDouble(newPrice.InnerHtml.Substring(1));
                }
                else
                {
                    price = Convert.ToDouble(priceContainer.SelectSingleNode("./p1").InnerHtml.Substring(1));
                }

                Product curProduct = new Product(this.WebsiteName, name, url, price, url, "");
                listOfProducts.Add(curProduct);
            }
        }

    }
}