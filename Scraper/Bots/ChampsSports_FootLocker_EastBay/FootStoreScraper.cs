﻿using System;
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

namespace StoreScraper.Scrapers.ChampsSports_FootLocker_EastBay
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

            string searchURL = UrlPrefix + string.Format(Keywords, settings.KeyWords) + PageSizeSuffix;
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.FireFoxHeaders);
            var document = request.GetDoc(searchURL, token);
            request.Dispose();
            var node = document.DocumentNode;
            HtmlNode container = node.SelectSingleNode("//*[@id=\"endeca_search_results\"]/ul");
            HtmlNodeCollection children = container.SelectNodes("./li");

            foreach (HtmlNode child in children)
            {
                
                    string id = child.GetAttributeValue("data-sku", null);
                    string name = child.SelectSingleNode(".//*[contains(@class, 'product_title')]")?.InnerText;
                    if(name == null) continue;
                    string link = child.SelectSingleNode("./a").GetAttributeValue("href", null);

                    var priceNode= child.SelectSingleNode(".//*[contains(@class, 'product_price')]");
                    string salePriceStr = priceNode.SelectSingleNode("./*[contains(@class, 'sale')]")?.InnerText;

                    string priceStr = (salePriceStr ?? priceNode.InnerText).Trim().Substring(1);
                    double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

                    var imgURL = child.SelectSingleNode("./a/img")?.GetAttributeValue("src", null);               
                    imgURL = imgURL?? child.SelectSingleNode("./a/span/img").GetAttributeValue("data-original", null);


                    Product product = new Product(this.WebsiteName, name, link, price, id, imgURL);
                    listOfProducts.Add(product);
            }

            token.ThrowIfCancellationRequested();
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var node = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.HtmlOnlyHeader).GetDoc(product.Url, token)
                .DocumentNode;
            HtmlNodeCollection sizes = node.SelectNodes("//*[@class=\"product_sizes\"]//*[@class=\"button\"]");
            ProductDetails details = new ProductDetails {SizesList = sizes.Select(size => size.InnerText).ToList()};
            return details;
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