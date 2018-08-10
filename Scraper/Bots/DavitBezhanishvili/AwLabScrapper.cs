using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.DavitBezhanishvili
{
    public class AwLabScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "aw-lab";
        public override string WebsiteBaseUrl { get; set; } = "https://en.aw-lab.com";
        public override bool Active { get; set; }
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                $"https://en.aw-lab.com/shop/catalogsearch/result/index/q/{settings.KeyWords}";
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            Logger.Instance.WriteErrorLog("Unexpected html!");
            var nodes = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'products-grid row')]");
            if (nodes == null)
            {
                return;
            }
            var children = nodes.SelectNodes("./div");
            if (children == null)
            {
                return;
            }

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child, settings);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, child, settings);
#endif
            }
        }


        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="settings"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child, settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private double getPrice(HtmlNode child)
        {
            string fullPriceString = child.SelectSingleNode(".//p[contains(@class,'small-price')]").InnerText;
            Price fullPrice = Utils.ParsePrice(fullPriceString);
            return fullPrice.Value;
        }

        ////////////////////////////////
        private string getImageUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class, 'product-image')]/a/img").GetAttributeValue("src", null);
        }
        /////////////////////////////////
        private string getProductUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class, 'product-image')]/a").GetAttributeValue("href", null);
        }
        //////////////////////////////////////
        private string getProductName(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class,product-data-container)]/h2/a").InnerText;
        }


        private string getCurrency(HtmlNode child)
        {
            string fullPriceString = child.SelectSingleNode(".//p[contains(@class,'small-price')]").InnerText;
            Price fullPrice = Utils.ParsePrice(fullPriceString);
            return fullPrice.Currency;
        }

        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {

            string imageURL = getImageUrl(child);
            string productURL = getProductUrl(child);
            string productName = getProductName(child);
            double price = getPrice(child);
            string currency = getCurrency(child);
            var product = new Product(this, productName, productURL, price, imageURL, productURL, currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

       
        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;
        }


        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var webPage = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();
            var jsonStr = Regex.Match(webPage.InnerHtml, @"var spConfig = new Product.Config\((.*)\)").Groups[1].Value;
            JObject parsed = JObject.Parse(jsonStr);

            var sizes = parsed.SelectToken("attributes").SelectToken("959").SelectToken("options");
            foreach (JToken sz in sizes.Children())
            {
                var sizeName = (string)sz.SelectToken("label");
                var size = parseSize(sizeName);
                details.AddSize(size, "Unknown");
            }
            return details;
        }

        private string parseSize(string sizeName)
        {
            var sizes = sizeName.Split(' ');
            if (sizes.Length == 1) return sizes[0];
            var numerator = Regex.Match(sizes[1], @"[\d]+").Groups[1].Value;
            var denominator = Regex.Match(sizes[1], @"[\d]+").Groups[2].Value;

            return sizes[0] + " " + numerator + "/" + denominator;
        }
    }
}
