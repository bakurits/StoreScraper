﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.GiorgiBaghdavadze._290sqm
{
    public class IstSqm : ScraperBase
    {
        public override string WebsiteName { get; set; } = "290sqm";
        public override string WebsiteBaseUrl { get; set; } = "https://ist.290sqm.com";
        public override bool Active { get; set; }

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            var searchUrl = "https://ist.290sqm.com/Just-Arrived";
            listOfProducts = new List<Product>();
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            Logger.Instance.WriteErrorLog("Unexpected html!");
            var children = document.DocumentNode.SelectNodes("//div[@id='content']/div[@class='row']/div");
            if (children == null)
            {
                return;
            }

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child, null);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, child, null);
#endif
            }
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                $"https://ist.290sqm.com/index.php?route=product/search&search={settings.KeyWords}";
            scrap(listOfProducts, searchUrl, token, settings);
        }


        private void scrap(List<Product> listOfProducts, String searchUrl, CancellationToken token, SearchSettingsBase settings)
        {
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            var a = document.DocumentNode.InnerHtml;
            Logger.Instance.WriteErrorLog("Unexpected html!");
            var children = document.DocumentNode.SelectNodes("//div[@id='main']/div[2]/div[@class='row']/div");
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
            string priceIntoString = child.SelectSingleNode(".//p[contains(@class,'price')]")?.InnerText;
            if (priceIntoString == null) return 0;
            Debug.Print(priceIntoString);
            string result = Regex.Match(priceIntoString, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }


        private string getImageUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//img[contains(@class,img-responsive)]")?.GetAttributeValue("src", null);
        }

        private string getProductUrl(HtmlNode child)
        {
            string url = child.SelectSingleNode(".//div[contains(@class,caption)]/h4/a")?.GetAttributeValue("href", null);
            return url;
        }

        private string getProductName(HtmlNode child)
        {
            string name = child.SelectSingleNode(".//div[contains(@class,caption)]/h4/a")?.InnerText;
            return name;
        }

        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            string imageURL = getImageUrl(child);
            string productURL = getProductUrl(child);
            string productName = getProductName(child);
            double price = getPrice(child);
            if (productName == null) return;
            var product = new Product(this, productName, productURL, price, imageURL, productURL, "TL");
            if (settings == null)
            {
                listOfProducts.Add(product);
                return;
            }
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }




        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            var asd = document.InnerHtml;
            const string xPath = "//select[@id='input-option11842']/option";
            var nodes = document.SelectNodes(xPath);
            if (nodes == null)
            {
                throw new Exception();
            }

            var sizes = nodes.Select(node => node.InnerText.Trim()).Where(element => !element.Contains("Seçiniz"));
            ProductDetails details = new ProductDetails();

            foreach (var size in sizes)
            {
                details.AddSize(size, "Unknown");
            }

            return details;

        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;

        }
    }
}
