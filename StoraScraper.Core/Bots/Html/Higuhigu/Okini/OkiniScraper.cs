using System;
using System.Collections.Generic;
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

namespace StoreScraper.Bots.Html.Higuhigu.Okini
{
    public class OkiniScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Okini";
        public override string WebsiteBaseUrl { get; set; } = "https://row.oki-ni.com";
        public override bool Active { get; set; }

        //private const string SearchFormat = @"https://row.oki-ni.com/catalogsearch/result/?q={0}&product_list_order=created_at";
        private const string SearchFormat = @"https://row.oki-ni.com/latest";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);

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
        
        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            FindItems(out listOfProducts, null, token);   
        }
        
        
        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to oki-ni website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var name = root.SelectSingleNode("//span[@itemprop='name']")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@class='price'][last()]");
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


            if (!root.InnerHtml.Contains("spConfig")) return result;
            var jsonStr = Regex.Match(root.InnerHtml, "\"spConfig\": (.*?),\n").Groups[1].Value;
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
            return result;
        }

        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token);
            return document;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = SearchFormat;
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to oki-ni website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//li[@class='item product product-item']");
            if (items != null) return items;
            Logger.Instance.WriteErrorLog("Unexpected Html!!");
            Logger.Instance.SaveHtmlSnapshop(document);
            throw new WebException("Unexpected Html");
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
            return item.SelectSingleNode(".//img").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            string priceStr = item.SelectSingleNode(".//span[@class='price'][last()]").InnerText;
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }
    }
}
