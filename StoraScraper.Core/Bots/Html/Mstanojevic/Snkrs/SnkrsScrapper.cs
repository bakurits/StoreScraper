﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Mstanojevic.Snkrs
{

    public class SnkrsScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "SNKRS";
        public override string WebsiteBaseUrl { get; set; } = "https://www.snkrs.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

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


        private readonly List<String> newArrivalPageUrls = new List<string>
        {
            "https://www.snkrs.com/en/166-new",
            "https://www.snkrs.com/en/2-sneakers",
            "https://www.snkrs.com/en/3-clothing",
            "https://www.snkrs.com/en/5-skate",
            "https://www.snkrs.com/en/181-lifestyle",
        };



        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            Task.WhenAll(newArrivalPageUrls.Select(url => GetProductsForPage(url, data, null, token))).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }


        private async Task GetProductsForPage(string url, ConcurrentDictionary<Product, byte> data,
            SearchSettingsBase settings, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var page = (await client.GetDocTask(url, token)).DocumentNode;
            HtmlNodeCollection collection = page.SelectNodes("//div[@class='product-container']");

            foreach (var item in collection)
            {
                token.ThrowIfCancellationRequested();
                Product product = GetProduct(item);
                if (product != null && (settings == null || Utils.SatisfiesCriteria(product, settings)))
                {
                    data.TryAdd(product, 0);
                }
            }

        }


        private Product GetProduct(HtmlNode item)
        {
            try
            {
                string name = GetName(item).TrimEnd();
                string url = GetUrl(item);
                var price = GetPrice(item);
                string imageUrl = GetImageUrl(item);
                return new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            }
            catch
            {
                return null;
            }
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


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            
            var price = Utils.ParsePrice(document.SelectSingleNode("//span[@id='our_price_display']").InnerText.Replace(",","."));
            



            string name = document.SelectSingleNode("//h1[@itemprop='name']").InnerText.Trim();
            string image = document.SelectSingleNode("//img[@id='bigpic']").GetAttributeValue("src", "");



            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency.Replace("&EURO;", "EUR"),
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };

            var strDoc = document.InnerHtml;

            if (strDoc.Contains("var combinations="))
            {

                var start = strDoc.IndexOf("var combinations=");


                var trimmed = strDoc.Substring(start, strDoc.Length - start);
                var end = trimmed.IndexOf(";");

                trimmed = trimmed.Substring(0, end);

                trimmed = trimmed.Replace("var combinations=", "");

                JObject obj = JObject.Parse(trimmed);
                foreach (var attr in obj)
                {

                    if (int.Parse(attr.Value["quantity"].ToString()) > 0)
                    {
                        details.AddSize(attr.Value["attributes_values"].First.First.ToString(), attr.Value["quantity"].ToString());

                    }



                }
            }

           /* var sizeCollection = document.SelectNodes("//span[@class='size_US']");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerHtml;
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
                }

            }
            */
            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            //string url = string.Format(SearchFormat, settings.KeyWords);
            string url = WebsiteBaseUrl + "/en/search?controller=search&orderby=position&orderway=desc&search_query="+settings.KeyWords.Replace(" ", "+")+"&submit_search=";
            //string url = WebsiteBaseUrl + "/en/166-new";

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//div[@class='product-container']");

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
            return true;
        }

        private string GetName(HtmlNode item)
        {
            //Console.WriteLine("GetName");
            //Console.WriteLine(item.SelectSingleNode("./a").GetAttributeValue("title", ""));

            return item.SelectSingleNode("./div/a/span[@class='product-name']").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*string priceDiv = item.SelectSingleNode("./div/a/span/span[@class='price product-price']").InnerHtml.Replace("€", "").Replace(",", ".");

            return double.Parse(priceDiv);*/

            return Utils.ParsePrice(item.SelectSingleNode("./div/a/span/span[@class='price product-price']").InnerHtml.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div/a/img").GetAttributeValue("src", null);
        }
    }
}
