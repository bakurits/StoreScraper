﻿using System;
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

namespace StoreScraper.Bots.Html.Higuhigu.Endclothing
{
    public class EndclothingScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Endclothing";
        public override string WebsiteBaseUrl { get; set; } = "https://www.endclothing.com";
        public override bool Active { get; set; }

        //private const string SearchFormat = @"https://www.endclothing.com/us/catalogsearch/result/?q={0}";
        private const string SearchFormat = @"https://www.endclothing.com/gb/latest-products/new-this-week";

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
                LoadSingleProductTryCatchWraper(listOfProducts, settings, item);
#endif
            }
        }

        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token);
            return document;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            if(document==null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to endclothing website");
                throw new WebException("Can't connect to website");
            }
            if (document == null)
            {
                
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//div[contains(@class, 'item product product-item')]");
            if (items == null)
            {
                Logger.Instance.WriteErrorLog("Uncexpected Html!!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Undexpected Html");
            }
            return items;
        }

        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
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
            return item.SelectSingleNode("./div[@class='product-item-info']/a/div/img[1]").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-item-info']/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            string priceStr = item.SelectSingleNode(".//span[@class='price'][last()]").InnerText;
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-item-info']/a/div/img[1]").GetAttributeValue("src", null);
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to endclothing website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var name = root.SelectSingleNode("//h1[@class='page-title']/span")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@class='price'][last()]");
            var price = Utils.ParsePrice(priceNode?.InnerText);
            var image = root.SelectSingleNode("//img[@class='swiper-slide']")?.GetAttributeValue("src", null);

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


            if (root.InnerHtml.Contains("spConfig"))
            {
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
            }
            return result;
        }
    }
}