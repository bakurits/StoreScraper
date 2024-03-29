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

namespace StoreScraper.Bots.Html.Mstanojevic.Triads
{

    public class TriadsScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Triads";
        public override string WebsiteBaseUrl { get; set; } = "https://www.triads.co.uk";
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
            "https://www.triads.co.uk/triads-mens-c1/accessories-c19",
            "https://www.triads.co.uk/triads-mens-c1/footwear-c24",

            "https://www.triads.co.uk/triads-mens-c1/t-shirts-c31",

            "https://www.triads.co.uk/new-products",
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
            HtmlNodeCollection collection = page.SelectNodes("//div[contains(@class,'product product--')]");

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
            var price = Utils.ParsePrice(document.SelectSingleNode("//span[@id='js-product-price']/span[@class='product-content__price--inc']/span").InnerText.Replace(",", "."));




            string name = document.SelectSingleNode("//span[@id='js-product-title']").InnerText.Trim();
            string image = WebsiteBaseUrl + document.SelectSingleNode("//img[@id='js-product-main-image']").GetAttributeValue("data-src", "");

            string brand = null;
            if (document.SelectSingleNode("//span[@class='product-content__title--brand']") != null)
            {
                brand = document.SelectSingleNode("//span[@class='product-content__title--brand']").InnerText;
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

            string id = productUrl.Substring(productUrl.Length - 5);
            string restApiUrl = "https://www.triads.co.uk/ajax/get_product_options/"+id;

            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var response = Utils.GetParsedJson(client, restApiUrl, token);

            foreach (var item in response["attributes"])
            {
                if (item["name"].ToString() == "UK Size")
                {
                    foreach (var value in item["values"])
                    {
                        if (int.Parse(value["stock_level"].ToString()) > 0)
                        {
                            details.AddSize(value["value"].ToString(), value["stock_level"].ToString());
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

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            //string url = string.Format(SearchFormat, settings.KeyWords);
            //string url = WebsiteBaseUrl + "/new-products/triads-mens-c1/footwear-c24";

            string url = WebsiteBaseUrl + "/ajax/getProductListings?base_url=search%2F"+settings.KeyWords.Replace(" ", "-")+"&page_type=productlistings&page_variant=show&all_upcoming_flag[]=78&keywords="+settings.KeyWords+"&show=&sort=&page=1&transport=html";

            //string url = WebsiteBaseUrl + "/new-products";

            /*if (settings.MaxPrice > 0)
            {
                url += "&min_price=" + settings.MinPrice.ToString() + "&max_price=" + settings.MaxPrice.ToString();
            }*/

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//div[contains(@class,'product product--')]");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./div/a").GetAttributeValue("title","").ToLower();
            var validKeywords = settings.KeyWords.ToLower().Split(' ');
            var invalidKeywords = settings.NegKeyWords.ToLower().Split(' ');
            foreach (var keyword in validKeywords)
            {
                if (!title.Contains(keyword))
                    return false;
            }


            foreach (var keyword in invalidKeywords)
            {
                if (keyword == "")
                    continue;
                if (title.Contains(keyword))
                    return false;
            }


            return true;

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            //if (!CheckForValidProduct(item, settings)) return;
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

            return item.SelectSingleNode("./div/a").GetAttributeValue("title", "");
        }

        private string GetUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode("./div/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*string priceDiv = item.SelectSingleNode("./div[3]/div/div/span/span/span/span").InnerHtml.Replace("$", "").Replace("£", "").Replace(",", ".");

            return double.Parse(priceDiv);*/
            return Utils.ParsePrice(item.SelectSingleNode("./div[3]/div/div/span/span/span/span").InnerHtml.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[2]/a/img").GetAttributeValue("src", null);
        }
    }
}
