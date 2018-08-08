using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Attributes;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Antonioli
{
    
    public class AntonioliScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Antonioli";
        public override string WebsiteBaseUrl { get; set; } = "https://www.antonioli.eu";
        public override bool Active { get; set; }

        public override Type SearchSettings { get; set; } = typeof(AntonioliSearchSettingsBase);


        private readonly string _searchformat = @"https://www.antonioli.eu/en/search?utf8=✓&q={0}&gender={1}";
        private static readonly string[] Gender = { "men", "women" };

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            AntonioliSearchSettingsBase.GenderEnum genderEnum = ((AntonioliSearchSettingsBase) settings).Gender;
            string gender = Gender[(int)genderEnum];
            string url = string.Format(_searchformat, settings.KeyWords, gender);
            var document = GetWebpage(url, token);
            HtmlNodeCollection itemCollection = document.SelectNodes("//*[@id='content']/section/article");
            listOfProducts = new List<Product>();

            foreach (HtmlNode item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
                Product product = GetProduct(item);
                if (product != null && Utils.SatisfiesCriteria(product, settings))
                {
                    listOfProducts.Add(product);
                }
            }

        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            product.Name = "";
            var page = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();
            HtmlNodeCollection collection = page.SelectNodes("//div[@id = 'product-variants']/div/label");
            foreach (var item in collection)
            {
                details.AddSize(item.InnerHtml, "Unknown");
            }
            var name = page.SelectSingleNode("//dd[@id = 'details']/span").InnerHtml;

            int ind = name.IndexOf("<br>", StringComparison.Ordinal);
            ind = ind == -1 ? name.Length : ind;
            name = name.Substring(0, ind);
            product.Name = name;
            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private Product GetProduct(HtmlNode item)
        {
            try
            {
                var url = GetUrl(item);
                var name = "default name";
                var imageUrl = GetImageUrl(item);
                var price = GetPrice(item);
                var currency = GetCurrency(item);
                return new Product(this, name, url, price, imageUrl, url, currency);
            }
            catch
            {
                return null;
            }
        }

        private string GetUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode("./a").GetAttributeValue("href", "");
        }
        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/figure/p/img").GetAttributeValue("src", "");
        }
        private double GetPrice(HtmlNode item)
        {
            HtmlNode priceContainer =
                item.SelectSingleNode("./a/figure/figcaption/div[contains(@class, 'price')]/span[1]");
            double.TryParse(priceContainer.GetAttributeValue("content", "0"), out var result);
            return result;
        }
        private string GetCurrency(HtmlNode item)
        {
            HtmlNode priceContainer =
                item.SelectSingleNode("./a/figure/figcaption/div[contains(@class, 'price')]/span[2]");
            string result = priceContainer.GetAttributeValue("content", "");
            return result;
        }
    }
}