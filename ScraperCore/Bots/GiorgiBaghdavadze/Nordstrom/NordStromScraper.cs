﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.GiorgiBaghdavadze.Nordstrom
{
    public class NordstromScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Nordstrom";
        public override string WebsiteBaseUrl { get; set; } = "http://shop.nordstrom.com";
        public override bool Active { get; set; }
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                $"http://shop.nordstrom.com/sr?origin=keywordsearch&keyword={settings.KeyWords}&top=72&offset=0&page=1&sort=Newest";
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            var ds = document.DocumentNode;
            var nodes = ds.SelectSingleNode("//div[contains(@class, 'resultSet_ZST5Lg')]/div");
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
                LoadSingleProductTryCatchWraper(listOfProducts,child,settings);
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
            string priceIntoString = child.SelectSingleNode(".//span[contains(@class,'price_Z1JgxME')]").InnerText;
            Debug.Print(priceIntoString);
            string result = Regex.Match(priceIntoString, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }


        private string getImageUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//img[contains(@class, 'image_12eiRp')]").GetAttributeValue("src", null);
        }

        private string getProductUrl(HtmlNode child)
        {
            string url = child.SelectSingleNode(".//a[contains(@class,link_22Nhi)]").GetAttributeValue("href", null);
            url = this.WebsiteBaseUrl + url;
            return url;
        }

        private string getProductName(HtmlNode child)
        {
            return child.SelectSingleNode(".//span[contains(@class,navigationLink_Z18Gs4y) and contains(@class, 'light_ZztMQb')]").InnerText;
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

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            string innerHtml = document.InnerHtml;
            int startIndx = document.InnerHtml.IndexOf("\"size\"" + ":[", StringComparison.Ordinal);
            if (startIndx == -1) return null;
            int endIndx = -1;
            endIndx = innerHtml.IndexOf("]", startIndx, StringComparison.Ordinal);
            if (endIndx == -1)
                return null;

            string jsonObjectStr = innerHtml.Substring(startIndx, endIndx - startIndx + 1);
            jsonObjectStr = jsonObjectStr.Substring(jsonObjectStr.IndexOf("[", StringComparison.Ordinal));
            JArray parsed = JArray.Parse(jsonObjectStr);

            string name = document.SelectSingleNode("//div[contains(@class, 'Z22ltwr')]/h1").InnerText;
            string priceIntoString = document.SelectSingleNode("//span[contains(@class, 'currentPriceString_PYXT2')]").InnerText;
            string result = Regex.Match(priceIntoString, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

            string imageURL = document.SelectSingleNode("//img[contains(@class, 'mainImage_Z2iFhqF')]").GetAttributeValue("src",null);
            ProductDetails details = new ProductDetails()
            {
                Name = name,
                Price = price,
                ImageUrl = imageURL,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };

            foreach (var x in parsed.Children())
            {
                var value = (string)x.SelectToken("displayValue");
                details.AddSize(value, "Unknown");
            }

            return details;
        }
    }
}
