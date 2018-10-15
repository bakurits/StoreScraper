using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Higuhigu.Consortium
{
    public class ConsortiumScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Consortium";
        public override string WebsiteBaseUrl { get; set; } = "https://www.consortium.co.uk";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.consortium.co.uk/latest";

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            string searchUrl = "http://www.consortium.co.uk/latest";
            HtmlNodeCollection itemCollection = GetProductCollection(token, searchUrl);

            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, null, item);
#else
                LoadSingleProductTryCatchWrapper(listOfProducts, null, item);
#endif
            }
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            const string url = SearchFormat;
            HtmlNodeCollection itemCollection = GetProductCollection(token, url);

            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item);
#else
                LoadSingleProductTryCatchWrapper(listOfProducts, settings, item);
#endif
            }

        }
        
        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to consortium website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var name = root.SelectSingleNode("//h1[@itemprop='name']")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode(".//span[@class='price'][last()]");
            var price = Utils.ParsePrice(priceNode?.InnerText);
            var image = root.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", null);

            ProductDetails result = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };


            if (root.InnerHtml.Contains("new Product.Config"))
            {
                var jsonStr = Regex.Match(root.InnerHtml, @"var spConfig = new Product.ConfigDefaultText\((.*)\)").Groups[1].Value;
                var tokenStr = Regex.Match(jsonStr, "\"(\\d+)\":").Groups[1].Value;
                JObject parsed = JObject.Parse(jsonStr);
                var sizes = parsed.SelectToken("attributes").SelectToken(tokenStr).SelectToken("options");
                foreach (JToken sz in sizes.Children())
                {
                    var sizeName = (string)sz.SelectToken("label");
                    JArray products = (JArray)sz.SelectToken("products");
                    if (products.Count > 0)
                    {
                        result.AddSize(sizeName, "Unknown");
                    }
                }
            }
            return result;
        }

        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token);
            return document;
        }

        private HtmlNodeCollection GetProductCollection(CancellationToken token, string url)
        {
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to consortium website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//li[@class='item text-center']");
            if (items == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected Html!!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexpected Html");
            }
            return items;
        }

        private void LoadSingleProductTryCatchWrapper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, item);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            var price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./img").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./img").GetAttributeValue("onclick", null).Split('=')[1].Replace("'", "");
        }

        private Price GetPrice(HtmlNode item)
        {
            string priceStr = item.SelectSingleNode(".//span[@class='price'][last()]") != null ? item.SelectSingleNode(".//span[@class='price'][last()]").InnerText : "0";
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./img").GetAttributeValue("src", null);
        }
    }
}
