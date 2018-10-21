using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Exceptions;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Bakurits.Kith
{
    public class KithScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Kith";
        public override string WebsiteBaseUrl { get; set; } = "https://kith.com/";
        public override bool Active { get; set; }


        private static readonly string[] Urls =
        {
            "https://kith.com/collections/latest",
            "https://kith.com/collections/womens-latest-products",
            "https://kith.com/collections/latest-kids"
        };


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> products = new ConcurrentDictionary<Product, byte>();
            var pages = Utils.GetPageTask(Urls.ToList(), token).Result;
            foreach (var page in pages)
            {
                var items = page.SelectNodes("//a[@class='product-card-info']");
                foreach (var item in items)
                {
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    LoadSingleProduct(products, null, item);
#else
                    LoadSingleProductTryCatchWrapper(listOfProducts, null, item);
#endif
                }
            }

            listOfProducts = products.Keys.ToList();
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var page = GetWebpage(productUrl, token);
            string name = page.SelectSingleNode("//h1[@class = 'product-header-title']").InnerText;
            name.EscapeNewLines();
            name = name.Trim();
            string priceContainer = page.SelectSingleNode("//span[@id = 'ProductPrice']").InnerText;
            var price = Utils.ParsePrice(priceContainer);
            var sizesContainer = page.SelectNodes("//select[@id = 'productSelect']/option");
            var keyWords = page.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", "") +
                           " " +
                           page.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", "");
            ProductDetails details = new ProductDetails()
            {
                Name = name,
                Price = price.Value,
                KeyWords = keyWords,
                Currency = price.Currency
            };
            foreach (var htmlNode in sizesContainer)
            {
                details.SizesList.Add((htmlNode.InnerHtml, "Unknown"));
            }

            return details;
        }

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo,
            CancellationToken token)
        {
            FindItems(out listOfProducts, null, token);
        }
        
        private static HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private void LoadSingleProductTryCatchWrapper(ConcurrentDictionary<Product, byte> listOfProducts,
            SearchSettingsBase settings, HtmlNode item)
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

        private void LoadSingleProduct(ConcurrentDictionary<Product, byte> listOfProducts, SearchSettingsBase settings,
            HtmlNode item)
        {
            var url = GetUrl(item);
            var name = GetName(item);
            var imageUrl = GetImageUrl(item);
            var price = GetPrice(item);
            listOfProducts.TryAdd(new Product(this, name, url, price.Value, imageUrl, url, price.Currency), byte.MaxValue);
        }


        private static string GetName(HtmlNode item)
        {
            string name = item.SelectSingleNode(".//p[contains(@class, 'product-card-title')]").InnerText;
            name.EscapeNewLines();
            name = name.Trim();
            return name;
        }

        private string GetUrl(HtmlNode item)
        {
            
            return WebsiteBaseUrl + item.GetAttributeValue("href", "");
        }

        private string GetImageUrl(HtmlNode item)
        {
            string imageUrl = item.ParentNode.SelectSingleNode(".//div[contains(@class, 'js-product-card-image-slideshow')]/a/img").GetAttributeValue("src", null);
            return "https:" + imageUrl;
        }

        private static Price GetPrice(HtmlNode item)
        {
            var priceContainer = item.SelectSingleNode(".//p[contains(@class, 'product-card-price')]").InnerText;
            return Utils.ParsePrice(priceContainer);
        }
    }
}