﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Xml;
using HtmlAgilityPack;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.ChampsSports_FootLocker_EastBay_FootAction
{
    [DisabledScraper]
    public class FootSimpleBase : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }

        private string UrlPrefix;
        private const string PageSizeSuffix = @"?Ns=P_NewArrivalDateEpoch%7C1&cm_SORT=New%20Arrivals";
        private const string Keywords = @"/keyword-{0}";
        private const string UlXpath = @"//*[@id=""endeca_search_results""]/ul";


        public FootSimpleBase(string websiteName, string websiteBaseUrl)
        {
            this.WebsiteName = websiteName;
            this.WebsiteBaseUrl = websiteBaseUrl;
            this.UrlPrefix = websiteBaseUrl + "/_-_";
        }

        private HtmlNode InitialNavigation(string url, CancellationToken token)
        {
            HttpClient ClientGenerator() => ClientFactory.GetProxiedFirefoxClient(autoCookies:true).AddHeaders(ClientFactory.FireFoxHeaders);
            var document = Utils.GetDoc(ClientGenerator, url, 3, 5, token);
            return document.DocumentNode;
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();

            string searchUrl = UrlPrefix + string.Format(Keywords, settings.KeyWords) + PageSizeSuffix;

            HtmlNode container = null;

            while (container == null)
            {
                HtmlNode node = InitialNavigation(searchUrl, token);
                container = node.SelectSingleNode(UlXpath);
            }

            HtmlNodeCollection children = container.SelectNodes("./li");

            foreach (HtmlNode child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, child);
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
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child)
        {
            string id = child.GetAttributeValue("data-sku", null);
            string name = child.SelectSingleNode(".//*[contains(@class, 'product_title')]")?.InnerText;
            if (name == null) return;
            string link = child.SelectSingleNode("./a").GetAttributeValue("href", null);

            var priceNode = child.SelectSingleNode(".//*[contains(@class, 'product_price')]");
            string salePriceStr = priceNode.SelectSingleNode("./*[contains(@class, 'sale')]")?.InnerText;

            string priceStr = (salePriceStr ?? priceNode.InnerText).Trim().Substring(1);
            int i = 0;

            for (i = 0; i < priceStr.Length; i++)
            {
                if (!((priceStr[i] >= '0' && priceStr[i] <= '9') || priceStr[i] == '.'))
                {
                    break;
                }
            }

            priceStr = priceStr.Substring(0, i);
            double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

            var imgUrl = child.SelectSingleNode("./a/img")?.GetAttributeValue("src", null);
            imgUrl = imgUrl ?? child.SelectSingleNode("./a/span/img").GetAttributeValue("data-original", null);

            Product product = new Product(this, name, link, price, id, imgUrl);
            listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient().AddHeaders(ClientFactory.HtmlOnlyHeader);
            var node = client.GetDoc(product.Url, token)
                .DocumentNode;
            HtmlNodeCollection sizes = node.SelectNodes("//*[@class=\"product_sizes\"]//*[@class=\"button\"]");
            ProductDetails details = new ProductDetails {SizesList = sizes.Select(size => size.InnerText).ToList()};
            return details;
        }

        public class ChampsSportsScraper : FootSimpleBase
        {
            public ChampsSportsScraper() : base("ChampsSports", "https://www.champssports.com")
            {
            }
        }

        public class EastBayScraper : FootSimpleBase
        {
            public EastBayScraper() : base("EastBay", "https://www.eastbay.com")
            {
            }
        }
    }

    [DisabledScraper]
    public class FootAPIBase : ScraperBase
    {

        public override string WebsiteName { get; set; }
        public override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }

        public FootAPIBase(string websiteName, string websiteBaseUrl)
        {
            this.WebsiteName = websiteName;
            this.WebsiteBaseUrl = websiteBaseUrl;
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();

            string searchUrl = WebsiteBaseUrl + $"/api/products/search?currentPage=0&pageSize=50&query={settings.KeyWords}&sort=newArrivals";
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies:true);
            var responseText = request.GetAsync(searchUrl, token).Result.Content.ReadAsStringAsync().Result;
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(responseText);
            var products = xmlDocument.SelectNodes("productCategorySearchPage/products");
            if (products == null)
            {
                Logger.Instance.WriteVerboseLog("[Error] Uncexpected XML!!");
                throw new WebException("Undexpected XML");
            }

            int sum = 0;

            foreach (XmlNode singleContact in products)
            {
                token.ThrowIfCancellationRequested();
                if (sum > 50) break;
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, singleContact, ref sum);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, singleContact, ref sum);
#endif
            }
        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="singleContact"></param>
        /// <param name="sum"></param>
        /// <param name="info"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings,
            XmlNode singleContact, ref int sum)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, singleContact, ref sum);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteVerboseLog(e.Message);
            }

        }

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="singleContact"></param>
        /// <param name="sum"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, XmlNode singleContact,
            ref int sum)
        {
            ++sum;
            XmlNode personalNode = singleContact.SelectSingleNode("name");
            string productName = personalNode?.InnerText;
            string imageUrl = singleContact.SelectSingleNode("images/url")?.InnerText;
            double.TryParse(singleContact.SelectSingleNode("price/value")?.InnerText, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var price);
            string sku = singleContact.SelectSingleNode("sku")?.InnerText;
            string link = new Uri(WebsiteBaseUrl + $"/product/{productName}/{sku}.html").AbsoluteUri;

            var product = new Product(this, productName, link, price, imageUrl, sku);
            listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {

            throw new NotImplementedException();
        }

        public class FootLockerScraper : FootAPIBase
        {
            public FootLockerScraper() : base("FootLocker", "https://www.footlocker.com")
            {
            }

        }

        public class FootActionScraper : FootAPIBase
        {
            public FootActionScraper() : base("FootAction", "https://www.footaction.com")
            {
            }
        }
    }
}