using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.GiorgiBaghdavadze
{
    class TresBienScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "TresBien";
        public override string WebsiteBaseUrl { get; set; } = "http://tres-bien.com";
        public override bool Active { get; set; }
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                $"http://tres-bien.com/search/?q={settings.KeyWords}";
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            Logger.Instance.WriteErrorLog("Unexpected html!");
            var nodes = document.DocumentNode.SelectSingleNode("//*[@id='kuLandingProductsListUl']");
            if (nodes == null)
            {
                return;
            }
            var children = nodes.SelectNodes("./li");
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
        /// <param name="info"></param>
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
            string priceIntoString = child.SelectSingleNode(".//div[contains(@class,'kuSalePrice')]/[2]").InnerText;
            Debug.Print(priceIntoString);
            string result = Regex.Match(priceIntoString, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }


        private string getImageUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class,klevuImgWrap)]/a/img").GetAttributeValue("src", null);
        }

        private string getProductUrl(HtmlNode child)
        {
            string url = child.SelectSingleNode(".//div[contains(@class,kuName)]").GetAttributeValue("href", null);
            return url;
        }

        private string getProductName(HtmlNode child)
        {
            string name = child.SelectSingleNode(".//div[contains(@class,kuName)]").InnerText;
            return name;
        }

        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            string imageURL = getImageUrl(child);
            string productURL = getProductUrl(child);
            string productName = getProductName(child);
            double price = getPrice(child);
            var product = new Product(this, productName, productURL, price, imageURL, productURL);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }
        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
