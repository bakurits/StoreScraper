using System;
using System.Collections.Generic;
using System.Net;
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
    public class AwLabScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Aw-lab";
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
                Logger.Instance.WriteErrorLog("Unexcepted Html");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexcepted Html");
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

        private string getImageUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class, 'product-image')]/a/img").GetAttributeValue("src", null);
        }

        private string getProductUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class, 'product-image')]/a").GetAttributeValue("href", null);
        }


        private string getProductName(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class,product-data-container)]/h2/a").InnerText;
        }


        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {

            var imageUrl = getImageUrl(child);
            var productUrl = getProductUrl(child);
            var productName = getProductName(child);
            var priceNode = child.SelectSingleNode(".//*[contains(@class, 'small-price')]");

            var priceStr = priceNode.SelectSingleNode("./del") != null
                ? priceNode.SelectSingleNode("./text()").InnerText
                : priceNode.InnerText;
            Price p = Utils.ParsePrice(priceStr);
            var product = new Product(this, productName, productUrl, p.Value, imageUrl, productUrl, p.Currency);
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
            var match = Regex.Match(sizes[1], @"([\d])");
            var numerator = match.Value;
            var denominator = match.NextMatch().Value;

            return sizes[0] + " " + numerator + "/" + denominator;
        }
    }
}
