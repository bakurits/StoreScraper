using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Attributes;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.ChampsSports_FootLocker_EastBay
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

            string searchURL = UrlPrefix + string.Format(Keywords,settings.KeyWords) + PageSizeSuffix;
            Console.WriteLine(searchURL);
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.HtmlOnlyHeader);

            var document = request.GetDoc(searchURL,token);

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

                    string imgURL = child.SelectSingleNode("./a/span/img").GetAttributeValue("data-original", null);

                    Product product = new Product(this.WebsiteName, name, link, price, id, imgURL);
                    listOfProducts.Add(product);
                }
                catch
                {
                    // ignored
                }
            }



            Console.WriteLine(container.ToString());
        }

        public List<string> GetProductSizes(Product product, CancellationToken token)
        {
            var node = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.HtmlOnlyHeader).GetDoc(product.Url, token)
                .DocumentNode;
            HtmlNodeCollection sizes = node.SelectNodes("//*[@class=\"product_sizes\"]//*[@class=\"button\"]");

            return sizes.Select(size => size.InnerText).ToList();
        }

        public class ChampsSportsScraper : FootStoreScraper
        {
            public ChampsSportsScraper() : base("ChampsSports", "https://www.champssports.com")
            {
            }
        }

        public class FootLockerScraper : FootStoreScraper
        {
            public FootLockerScraper() : base("FootLocker", "https://www.footlocker.com")
            {
            }
        }

        public class EastBayScraper : FootStoreScraper
        {
            public EastBayScraper() : base("EastBay", "https://www.eastbay.com")
            {
            }
        }
    }
}