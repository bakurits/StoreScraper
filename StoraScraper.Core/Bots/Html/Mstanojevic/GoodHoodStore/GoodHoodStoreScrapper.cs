﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Mstanojevic.GoodHoodStore
{

    public class GoodHoodStoreScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "GoodHoodStore";
        public override string WebsiteBaseUrl { get; set; } = "https://www.goodhoodstore.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, "", token);
            if (itemCollection == null)
                return;
            Console.Write(itemCollection.Count);
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


        private readonly List<String> newArrivalPageUrls = new List<string>
        {
            "https://goodhoodstore.com/mens/latest?n=all",
            "https://goodhoodstore.com/mens/all-mens-clothing?n=all",
            "https://goodhoodstore.com/mens/all-mens-footwear?n=all",
            "https://goodhoodstore.com/mens/all-mens-accessories?n=all",
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
            HtmlNodeCollection collection = page.SelectNodes("//div[@class='overview']");

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


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            var price = Utils.ParsePrice(document.SelectSingleNode("//p[@class='Price']/span/span[1]").InnerHtml);


            string name = document.SelectSingleNode("//h1[@class='Title']").InnerText.Trim();
            string image = document.SelectSingleNode("//div[@class='imgs']/div/a/img").GetAttributeValue("src", "");

            name = Regex.Replace(name, @"\s+", " ");

            string brand = null;
            if (document.SelectSingleNode("//span[@id='Brand-Title']") != null)
            {
                brand = document.SelectSingleNode("//span[@class='Brand-Title']").InnerText;
            }

            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency.Replace("&EURO;","EUR"),
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this,
                BrandName = brand
            };


            var sizeCollection = document.SelectNodes("//select[@name='id']/option");
            if (sizeCollection != null)
            {
                foreach (var size in sizeCollection)
                {
                    if (!size.InnerText.Contains("Out of Stock"))
                    {
                        string sz = size.InnerHtml;
                        if (sz.Contains("Select"))
                        {
                            continue;
                        }

                        if (sz.Length > 0)
                        {
                            details.AddSize(sz, "Unknown");
                        }
                    }
                }
            }

            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, string gender, CancellationToken token)
        {
            //string url = string.Format(SearchFormat, settings.KeyWords);
            //string url = WebsiteBaseUrl + "/search?n=all&q=" + settings.KeyWords;
            
            string url = WebsiteBaseUrl + "/search?n=all&q=" + settings.KeyWords;
            
            //women link /womens/footwear

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//div[@class='overview']");

        }

       

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            //if (!CheckForValidProduct(item, settings)) return;
            if (item.SelectSingleNode("./p/span[@class='Price']/span") == null)
                return;
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            var price = GetPrice(item);

            /*if (!(price >= settings.MinPrice && price <= settings.MaxPrice))
            {
                return;
            }*/


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

            return item.SelectSingleNode("./p/span[@class='Title']").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /* string priceDiv = item.SelectSingleNode("./p/span[@class='Price']/span").InnerHtml.Replace("€", "").Replace("&euro;", ""); ;

             return double.Parse(priceDiv);*/
            return Utils.ParsePrice(item.SelectSingleNode("./p/span[@class='Price']/span").InnerHtml.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null);
        }
    }
}
