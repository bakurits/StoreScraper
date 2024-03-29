﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Mstanojevic.UebervartShop
{
    public class UebervartShopScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "UebervartShop";
        public override string WebsiteBaseUrl { get; set; } = "https://www.uebervart-shop.de";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
            Console.WriteLine(itemCollection.Count);
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
            "https://www.uebervart-shop.de/new/",
            "https://www.uebervart-shop.de/shoes/",
            "https://www.uebervart-shop.de/outerwear/",
            "https://www.uebervart-shop.de/accessoires/",
            "https://www.uebervart-shop.de/shirts/"
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
            HtmlNodeCollection collection = page.SelectNodes("//article");

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


            Price price;

            if (document.SelectSingleNode("//ins/span[@class='woocommerce-Price-amount amount']") != null)
            {
                price = Utils.ParsePrice(document.SelectSingleNode("//ins/span[@class='woocommerce-Price-amount amount']").InnerText.Replace(",", ".").Replace("&nbsp;", ""));

            }
            else
            {
                price = Utils.ParsePrice(document.SelectSingleNode("//span[@class='woocommerce-Price-amount amount']").InnerText.Replace(",", ".").Replace("&nbsp;", ""));

            }



            string name = document.SelectSingleNode("//h3[@class='product_title']").InnerText.Trim();
            string image = document.SelectSingleNode("//div[@class='swiper-slide']/img").GetAttributeValue("src", "");


            string brand = null;
            if (document.SelectSingleNode("//meta[@property='og:brand']") != null)
            {
                brand = document.SelectSingleNode("//meta[@property='og:brand']").GetAttributeValue("content", null);
            }

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

            var sizeCollection = document.SelectNodes("//td[@class='value']/label");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerHtml;
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
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

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            //string url = string.Format(SearchFormat, settings.KeyWords);
            string url = WebsiteBaseUrl + "/?s=" + settings.KeyWords.Replace(" ", "+") + "&post_type=product";
            //string url = WebsiteBaseUrl + "/new";

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//article");

        }





        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {

            if (item.SelectSingleNode("./a/div[@class = 'soldout']") != null)
                return false;


            return true;

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (!CheckForValidProduct(item, settings))
                return;

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

            return item.SelectSingleNode("./a/h3").InnerText;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            try
            {
                if (item.SelectSingleNode("./a/span/ins/span") != null)
                {
                Console.WriteLine(item.SelectSingleNode("./a/span/ins/span").InnerText.Replace("&nbsp;", "").Replace("\"", ""));
                    return Utils.ParsePrice(item.SelectSingleNode("./a/span/ins/span").InnerText.Replace("&nbsp;", "").Replace("\"",""), ",", ".");
                }
                else
                {
                Console.WriteLine(item.SelectSingleNode("./a/span/span").InnerText.Replace("&nbsp;", "").Replace("\"", ""));

                return Utils.ParsePrice(item.SelectSingleNode("./a/span/span").InnerText.Replace("&nbsp;", "").Replace("\"", ""), ",", ".");
                }
            }
            catch
            {
               
                    return new Price()
                    {
                        Value = 0,
                        Currency = "UNKNOWN"
                    };
                
            }
            
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null);
        }
    }
}

