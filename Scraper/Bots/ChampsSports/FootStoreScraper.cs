using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Flurl.Http;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.ChampsSports
{
    public class FootStoreScraper : ScraperBase
    {
        public override string WebsiteName { get; set; }
        public override string WebsiteBaseUrl { get; set; }
        public override bool Enabled { get; set; }

        private string UrlPrefix;
        private const string PageSizeSuffix = @"?Ns=P_NewArrivalDateEpoch%7C1&cm_SORT=New%20Arrivals";
        private const string Keywords = @"/keyword-{0}";

        private Object Header = new { Accept = "text/html" };


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
            info.WriteLog(searchURL);
            var request = searchURL.WithHeaders(Header);

            var document = request.GetDoc(token);

            var node = document.DocumentNode;

            HtmlNode container = node.SelectSingleNode("//*[@id=\"endeca_search_results\"]/ul");
            HtmlNodeCollection children = container.SelectNodes("./li");

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

                    string imgURL;
                    try
                    {
                        imgURL = child.SelectSingleNode("./a/img").GetAttributeValue("src", null);

                    }
                    catch (Exception e)
                    {
                        imgURL = child.SelectSingleNode("./a/span/img").GetAttributeValue("data-original", null);

                    }

                    /*
                    Console.WriteLine(id);
                    Console.WriteLine(name);
                    Console.WriteLine(link);
                    Console.WriteLine(price);
                    Console.WriteLine(imgURL);
                    Console.WriteLine();
                    */

                    Product product = new Product(this.WebsiteName, name, link, price, id, imgURL);
                    listOfProducts.Add(product);
                }
                catch (Exception e)
                {
                    info.WriteLog(e.StackTrace);
                    info.WriteLog(@"This is not a product!");
                }
            }



            Console.WriteLine(container.ToString());
        }

        public string addKeywords()
        {
            return "blue";
        }

        public List<string> GetProductSizes(Product product, CancellationToken token)
        {
            List<string> productSizes = new List<string>();
            var node = product.Url.WithHeaders(Header).GetDoc(token).DocumentNode;
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
}