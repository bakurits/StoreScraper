﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Higuhigu.Solebox
{
    public class SoleboxScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Solebox";
        public override string WebsiteBaseUrl { get; set; } = "https://www.solebox.com";
        public override bool Active { get; set; }

        //private const string SearchFormat = @"https://www.solebox.com/en/variant/?ldtype=grid&_artperpage=240&listorderby=oxinsert&listorder=desc&pgNr=0&searchparam={0}";
        private const string SearchFormat = @"https://www.solebox.com/en/New/";
        private const string NoResults = "Sorry, no results found for your searchterm";

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
                Logger.Instance.WriteErrorLog($"Can't Connect to solebox website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var sizeNodes = root.SelectNodes("//div[@class='sizeBlock']/div/a");
            var sizes = sizeNodes?.Select(node => node?.GetAttributeValue("data-size-eu", null)).ToList();

            var name = root.SelectSingleNode("//h1[@id='productTitle']/span")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode(".//div[@id='productPrice']");
            var price = Utils.ParsePrice(priceNode?.InnerText.Replace(",", "."));
            var image = root.SelectSingleNode("//a[@id='zoom1']")?.GetAttributeValue("src", null);

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

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            const string url = SearchFormat;
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to solebox website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            if(node.InnerHtml.Contains(NoResults))
            {
                return null;
            }
            var items = node.SelectNodes("//li[@class='productData']");
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

        private bool GetStatus(HtmlNode item)
        {
            return item.SelectNodes(".//div[@class='release']") != null;
        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./a/div[@class='titleBlock title']").SelectNodes("./div")[1].InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            string priceStr = item.SelectSingleNode(".//div[contains(@class, 'priceContainer')]").InnerHtml.Split('<')[0].Replace(",", ".");
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/div[@class='gridPicture']/img").GetAttributeValue("src", null);
        }
    }
}
