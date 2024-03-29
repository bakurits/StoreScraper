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

namespace StoreScraper.Bots.Html.Mstanojevic.Dtlr
{

    public class DtlrScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "DTLR";
        public override string WebsiteBaseUrl { get; set; } = "https://www.dtlr.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {


            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, null, token);
            if (itemCollection == null)
            {
                return;
            }
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
            "https://www.dtlr.com/men/footwear/new.html",
            "https://www.dtlr.com/men/apparel/new.html",
            "https://www.dtlr.com/men/accessories/new.html",
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
            HtmlNodeCollection collection = page.SelectNodes("//div[@class='product']");

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
            if (document.SelectSingleNode("//p[@class='special-price']/span[@class='price']") != null) { 
                price = Utils.ParsePrice(document.SelectSingleNode("//p[@class='special-price']/span[@class='price']").InnerText);
            }else{
                price = Utils.ParsePrice(document.SelectSingleNode("//div[@class='price-info']/div[@class='price-box']/span[@class='regular-price']/span").InnerText);
            }
            string name = document.SelectSingleNode("//h1[@itemprop='name']").InnerText;
            string image = document.SelectSingleNode("//div[@class='product-image-gallery']/img[1]").GetAttributeValue("src", "");

            string brand = null;
            if (document.SelectSingleNode("//span[@itemprop='brand']") != null)
            {
                brand = document.SelectSingleNode("//span[@itemprop='brand']").InnerText;
            }
            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this,
                BrandName = brand
            };

            var strDoc = document.InnerHtml;

            if (strDoc.Contains("var spConfig = new Product.Config({"))
            {
                var start = strDoc.IndexOf("var spConfig = new Product.Config({");
                var trimmed = strDoc.Substring(start, strDoc.Length - start);
                var end = trimmed.IndexOf(");");

                trimmed = trimmed.Substring(0, end);

                trimmed = trimmed.Replace("var spConfig = new Product.Config(", "");
                JObject obj = JObject.Parse(trimmed);

                foreach (var attr in obj["attributes"])
                {

                    foreach (var x in attr)
                    {
                        if (x["code"].ToString() == "size")
                        {
                            foreach (var option in x["options"])
                            {
                                details.AddSize(option["label"].ToString(), "Unknown");
                            }
                        }


                    }
                }

            }
            // need to parse javascript to extract prices
            /*var sizeCollection = document.SelectNodes("//div[@class='sizeBox']/ul/li");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerHtml;
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
                }

            }*/

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
            string url = "";

            //string url = WebsiteBaseUrl + "/catalogsearch/result/?q="+settings.KeyWords.Replace(" ", "+");
            
            url = WebsiteBaseUrl + "/catalogsearch/result/?q=" + settings.KeyWords.Replace(" ", "+");
            if (gender != null)
            {
                url += "&gender=" + gender;
            }
            


            

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//div[@class='product']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode(".//h3[@class='product-name']").InnerText.ToLower();
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

            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);

            
            var price = GetPrice(item);

            /*if (!(price >= settings.MinPrice && price <= settings.MaxPrice) && (settings.MaxPrice != 0 && settings.MinPrice != 0))
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

            return item.SelectSingleNode(".//h3[@class='product-name']").InnerText;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*var node = item.SelectSingleNode("./div/div[@class='price-box']/span/span");
            if (node != null)
            {
                string priceDiv = item.SelectSingleNode("./div/div[@class='price-box']/span/span").InnerHtml.Replace("€", "").Replace("&euro;", "").Replace("$", "");

                return double.Parse(priceDiv);
            }
            else
            {
                return 0;
            }*/
            
                return Utils.ParsePrice(item.SelectSingleNode(".//div[@class='price-box']/span/span").InnerHtml);
            
            }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a//img").GetAttributeValue("src", null);
        }
    }
}
