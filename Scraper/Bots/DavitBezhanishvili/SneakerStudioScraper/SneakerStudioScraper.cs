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
using System.Net;
using System.Net.Http;

namespace StoreScraper.Bots.DavitBezhanishvili.SneakerStudioScraper
{
    public class SneakerStudioScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "SneakerStudio";
        public override string WebsiteBaseUrl { get; set; } = "https://sneakerstudio.com";
        public override bool Active { get; set; }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                new Uri("https://sneakerstudio.com/settings.php?sort_order=date-d&curr=USD");

            var referer = new Uri($"https://sneakerstudio.com/search.php?text={settings.KeyWords}");

            var client = ClientFactory.GetProxiedFirefoxClient();
            HttpRequestMessage message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.RequestUri = searchUrl;
            message.Headers.Referrer = referer;
           
            var document = client.GetDoc(message, token);
            var nodes = document.DocumentNode.SelectSingleNode("//div[@class = 'row']");
            if (nodes == null)
            {
                Logger.Instance.WriteErrorLog("Unexcepted Html");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexcepted Html");
            }
            var children = nodes.SelectNodes("./div[@class = 'product_wrapper col-md-4 col-xs-6']");
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
            var picNode = child.SelectSingleNode("./a[@class = 'product-icon']");
            return WebsiteBaseUrl + picNode.SelectSingleNode("./img").GetAttributeValue("data-src",null);
        }

        private string getProductUrl(HtmlNode child)
        {
            return WebsiteBaseUrl + child.SelectSingleNode("./a[@class = 'product-icon']").GetAttributeValue("href", null);
        }


        private string getProductName(HtmlNode child)
        {
            return child.SelectSingleNode("./h3/a").InnerText;
        }


        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            Price p = getProductPrice(child);
            var imageUrl = getImageUrl(child);
            var productUrl = getProductUrl(child);
            var productName = getProductName(child);

            var product = new Product(this, productName, productUrl, p.Value, imageUrl, productUrl, p.Currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private Price getProductPrice(HtmlNode child)
        {
            var priceNode = child.SelectSingleNode("./div[@class = 'product_prices']/span[@class = 'price']");
            string priceStr = priceNode.SelectSingleNode("./text()").InnerText;
            Price p = Utils.ParsePrice(priceStr);
            return p;
        }


        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
           return null;
        }

        

      
    }
}
