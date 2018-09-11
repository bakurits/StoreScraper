using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using System.Net.Http;
using StoreScraper.Attributes;

namespace StoreScraper.Bots.DavitBezhanishvili.SneakerStudioScraper
{
    public class SneakerStudioScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "SneakerStudio";
        public override string WebsiteBaseUrl { get; set; } = "http://sneakerstudio.com";
        public override bool Active { get; set; }

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl = "http://sneakerstudio.com/settings.php?sort_order=date-d&curr=USD";
            var client = ClientFactory.GetProxiedFirefoxClient();

            var document = client.GetDoc(searchUrl, token);
            Scrap(document, ref listOfProducts, null, token);

        }

        private ConcurrentDictionary<HttpClient, DateTime> activeClients = new ConcurrentDictionary<HttpClient, DateTime>();

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                new Uri("http://sneakerstudio.com/settings.php?sort_order=date-d&curr=USD");

            var referer = new Uri($"http://sneakerstudio.com/search.php?text={settings.KeyWords}");

            var client = ClientFactory.GetProxiedFirefoxClient();
            HttpRequestMessage message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.RequestUri = searchUrl;
            message.Headers.Referrer = referer;
            activeClients.TryGetValue(client, out var value);

            if (DateTime.Now.Subtract(value).TotalMinutes > 10)
            {
                var resp = client.SendAsync(message, token).Result;
                resp.EnsureSuccessStatusCode();
                resp.Dispose();
                activeClients.AddOrUpdate(client, DateTime.Now, (httpClient, time) => DateTime.Now);
            }


            var document = client.GetDoc(referer.AbsoluteUri, token);
            Scrap(document, ref listOfProducts, settings, token);
        }

        private void Scrap(HtmlDocument document, ref List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {

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
                if (child.SelectSingleNode("./a[@class = 'product-icon']/div[@class ='comingsoon']") == null)
                {
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    LoadSingleProduct(listOfProducts, child, settings);
#else
                    LoadSingleProductTryCatchWraper(listOfProducts, child, settings);
#endif
                }

            listOfProducts = (from prod in listOfProducts.AsParallel()
                              select string.IsNullOrWhiteSpace(prod.Name) ? (Product)GetProductDetails(prod.Url, token) : prod).ToList();
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
            return WebsiteBaseUrl + picNode.SelectSingleNode("./img").GetAttributeValue("data-src", null);
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
            var searchUrl =
                new Uri("http://sneakerstudio.com/settings.php?curr=USD");

            var referer = new Uri(url);

            var client = ClientFactory.GetProxiedFirefoxClient();
            HttpRequestMessage message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.RequestUri = searchUrl;
            message.Headers.Referrer = referer;

            return client.GetDoc(message, token).DocumentNode;
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var webPage = GetWebpage(productUrl, token);
            ProductDetails details = ConstructProduct(webPage, productUrl);

            var jsonStr = getJson(webPage.InnerHtml);
            JObject parsed = JObject.Parse(jsonStr);


            var sizes = parsed.SelectTokens("sizes");
            foreach (JToken szToken in sizes.Children())
            {
                var sz = szToken.First;
                var size = (string)sz.SelectToken("name");
                var amount = (string)sz.SelectToken("amount");
                if (amount != "0")
                    details.AddSize(size, amount);
            }
            return details;
        }

        private string getJson(string webPageInnerHtml)
        {
            string json = "{";
            var str = "var product_data = {";
            var ind = webPageInnerHtml.IndexOf(str);
            ind += str.Length;
            int parCount = 1;
            while (parCount > 0)
            {
                json += webPageInnerHtml[ind];
                if (webPageInnerHtml[ind] == '}')
                    parCount--;
                if (webPageInnerHtml[ind] == '{')
                    parCount++;
                ind++;
            }

            return json;
        }

        private ProductDetails ConstructProduct(HtmlNode webPage, string productUrl)
        {
            var name = webPage.SelectSingleNode("//div[@class='projector_navigation']/h1").InnerText.Trim();

            var image = WebsiteBaseUrl + webPage.SelectSingleNode(
                            "//div[contains(@class,'photos col-md-7 col-xs-12')]/a[@id ='projector_image_1']/img").GetAttributeValue("src", null);
            var priceNode = webPage.SelectSingleNode("//strong[@class='projector_price_value']");
            var txt = priceNode.InnerText.Trim();
            var price = Utils.ParsePrice(txt);
            var keyWords = webPage.SelectSingleNode("//meta[@name = 'keywords']").GetAttributeValue("content", null);

            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                KeyWords = keyWords,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };
            return details;
        }


    }
}
