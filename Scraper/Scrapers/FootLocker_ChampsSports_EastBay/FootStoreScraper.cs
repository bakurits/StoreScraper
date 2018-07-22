using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Interfaces;
using StoreScraper.Models;

namespace StoreScraper.Scrapers.FootLocker_ChampsSports_EastBay
{
    [DisabledScraper]
    public class FootStoreScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Enabled { get; set; }

        private string UrlPrefix;
        private const string PageSizeSuffix = @"?Ns=P_NewArrivalDateEpoch%7C1&cm_SORT=New%20Arrivals";
        private const string Keywords = @"/keyword-{0}";

        public FootStoreScraper(string websiteName, string websiteBaseUrl)
        {
            this.WebsiteName = websiteName;
            this.WebsiteBaseUrl = websiteBaseUrl;
            this.UrlPrefix = websiteBaseUrl + "/_-_";
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, Logger info)
        {
            listOfProducts = new List<Product>();

            string searchURL = UrlPrefix + string.Format(Keywords, addKeywords()) + PageSizeSuffix;
            Console.WriteLine(searchURL);
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.AcceptHtmlOnlyHeader);

            var document = request.GetDoc(searchURL, token);

            var node = document.DocumentNode;
            HtmlNode container = node.SelectSingleNode("//*[@id=\"endeca_search_results\"]/ul");
            HtmlNodeCollection children = container.SelectNodes("./li");

            Console.WriteLine(searchURL);
            foreach (HtmlNode child in children)
            {
                try
                {
                    string id = child.GetAttributeValue("data-sku", null);
                    string name = child.SelectSingleNode(".//*[contains(@class, 'product_title')]").InnerText;
                    string link = child.SelectSingleNode("./a").GetAttributeValue("href", null);

                    string priceStr = child.SelectSingleNode(".//*[contains(@class, 'product_price')]").InnerText;
                    priceStr = priceStr.Trim().Substring(1);
                    double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

                    //string imgURL = child.SelectSingleNode("./a/span/img").GetAttributeValue("data-original", null);

                    //Product product = new Product(this.WebsiteName, name, link, price, id, imgURL);
                }
                catch 
                {
                   //ignore
                }

                //GetProductSizes(product, token);
            }



            Console.WriteLine(container.ToString());
        }

        public string addKeywords()
        {
            return "blue";
        }

        public List<string> GetProductSizes(Product product, CancellationToken token)
        {
            List <string> productSizes = new List<string>();
            var node = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.AcceptHtmlOnlyHeader).GetDoc(product.Url, token).DocumentNode;
            Console.WriteLine(node.InnerHtml);
            HtmlNodeCollection sizes = node.SelectNodes("//*[@class=\"product_sizes\"]//*[@class=\"button\"]");
            foreach (HtmlNode size in sizes)
            {
                Console.WriteLine(size.InnerText);
                productSizes.Add(size.InnerText);
            }

            return productSizes;
        }
    }

    public class ChampsSportsScraper : FootStoreScraper
    {
        public ChampsSportsScraper() : base("ChamspSports", "https://www.champssports.com"){}
    }

    public class FootLockerScraper : FootStoreScraper
    {
        public FootLockerScraper() : base("FootLocker", "https://www.footlocker.com") { }
    }

    public class EastBayScraper : FootStoreScraper
    {
        public EastBayScraper() : base("EastBay", "https://www.eastbay.com") { }
    }
}