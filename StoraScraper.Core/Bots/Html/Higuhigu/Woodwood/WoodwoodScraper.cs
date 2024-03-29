﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace StoreScraper.Bots.Html.Higuhigu.Woodwood
{
    public class WoodwoodScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Woodwood";
        public override string WebsiteBaseUrl { get; set; } = "https://www.woodwood.com/";
        public override bool Active { get; set; }
        public override Type SearchSettingsType { get; set; } = typeof(WoodwoodSearchSettings);
        private static readonly string[] Links = { "https://www.woodwood.com/men/new-arrivals", "https://www.woodwood.com/women/new-arrivals" };

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            WoodwoodSearchSettings.GenderEnum genderEnum;
            try
            {
                genderEnum = ((WoodwoodSearchSettings)settings).Gender;
            }
            catch
            {
                genderEnum = WoodwoodSearchSettings.GenderEnum.Both;
            }
            listOfProducts = new List<Product>();
            switch (genderEnum)
            {
                case WoodwoodSearchSettings.GenderEnum.Man:
                    FindItemsForGender(listOfProducts, settings, token, 0);
                    break;
                case WoodwoodSearchSettings.GenderEnum.Woman:
                    FindItemsForGender(listOfProducts, settings, token, 1);
                    break;
                default:
                    FindItemsForGender(listOfProducts, settings, token, 0);
                    FindItemsForGender(listOfProducts, settings, token, 1);
                    break;
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
                Logger.Instance.WriteErrorLog($"Can't Connect to woodwood website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            
            var sizeNodes = root.SelectNodes("//select[contains(@id, 'form-size')]//option[not(@value='')]");
            var sizes = GetSizes(root.InnerHtml);

            var name = root.SelectSingleNode("//h1[@class='headline']")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@class='price']");
            var price = Utils.ParsePrice(priceNode?.InnerText);
            var image = root.SelectSingleNode("//a[@id='commodity-show-image']/img")?.GetAttributeValue("src", null);

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

            Debug.Assert(sizes != null, nameof(sizes) + " != null");
            foreach (var size in sizes)
            {
                result.AddSize(size, "Unknown");
            }

            return result;
        }


        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token);
            return document;
        }

        private void FindItemsForGender(List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, int gender)
        {
            string url = Links[gender];
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to woodwood website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//ul[@id='commodity-lister-list']/li");
            if (items == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected Html!!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexpected Html");
            }
            foreach (var item in items)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item);
#else
                LoadSingleProductTryCatchWrapper(listOfProducts, settings, item);
#endif
            }
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
            if (item.InnerHtml.Contains("Coming soon")) return;
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
            return item.SelectSingleNode(".//img").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            string priceStr = item.SelectSingleNode(".//span[@class='list-commodity-price']") != null ? 
                                item.SelectSingleNode(".//span[@class='list-commodity-price']").InnerText : 
                                item.SelectSingleNode(".//span[@class='list-commodity-offer']").InnerText;
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }

        private List<string> GetSizes(string html)
        {
            var indexOf = html.IndexOf("'items':",
                StringComparison.Ordinal);
            var json = Utils.GetFirstJson(html.Substring(indexOf));
            List<string> ans = new List<string>();
            foreach (var jToken in json)
            {
                try
                {
                    var size = jToken.Value["params"][1].ToString();
                    if (!size.Contains("Out Of Stock"))
                    {
                        ans.Add(size);
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }

            }

            return ans;
        }
    }
}
