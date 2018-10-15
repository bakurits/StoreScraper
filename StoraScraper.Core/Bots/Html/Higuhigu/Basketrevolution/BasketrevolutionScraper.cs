using System;
using System.Collections.Generic;
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

namespace StoreScraper.Bots.Html.Higuhigu.Basketrevolution
{
    public class BasketrevolutionScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Basketrevolution";
        public override string WebsiteBaseUrl { get; set; } = "https://www.basketrevolution.es";
        public override bool Active { get; set; }
        
        private const string SearchFormat = @"https://www.basketrevolution.es/catalogsearch/result/index/?dir=asc&limit=all&order=created_at&q={0}";
        private static readonly string[] Links = { "https://www.basketrevolution.es/shoes/basket-junior/"/*,
                                                    "https://www.basketrevolution.es/shoes/training/?___SID=U&limit=all",
                                                    "https://www.basketrevolution.es/shoes/training/?___SID=U&limit=all"*/
        };


        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var pages = Utils.GetPageTask(Links.ToList(), token).Result;
            foreach (var page in pages)
            {
                var items = page.SelectNodes("//div[@class='item-box']");
                foreach (var item in items)
                {
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    LoadSingleProduct(listOfProducts, null, item);
#else
                    LoadSingleProductTryCatchWrapper(listOfProducts, null, item);
#endif
                }
            }
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            string url = string.Format(SearchFormat, settings.KeyWords);
            listOfProducts = new List<Product>();
            Scrap(listOfProducts, settings, token, url);
        }
        
        
        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to basketrevolution website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;           
            var name = root.SelectSingleNode("//h1[@class='product-name']")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@class='price']");
            var price = Utils.ParsePrice(priceNode?.InnerText.Replace(",", "."));
            var image = root.SelectSingleNode("//img[@id='image']")?.GetAttributeValue("src", null);

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

            if (!root.InnerHtml.Contains("new Product.Config")) return result;
            
            var jsonStr = Regex.Match(root.InnerHtml, @"var spConfig = new Product.Config\((.*)\)").Groups[1].Value;
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

        private void Scrap(List <Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, string url)
        {
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
                Logger.Instance.WriteErrorLog($"Can't Connect to basketrevolution website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//div[@class='item-box']");
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
            if (settings == null || Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-image']/a").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-image']/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        { 
            string priceStr = item.SelectSingleNode(".//span[@class='price']").InnerText.Replace(",", ".");
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-image']/a/img").GetAttributeValue("src", null);
        }
    }
}
