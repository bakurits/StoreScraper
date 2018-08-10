using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Bakurits.Antonioli
{
    
    public class AntonioliScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Antonioli";
        public override string WebsiteBaseUrl { get; set; } = "https://www.antonioli.eu";
        public override bool Active { get; set; }

        public override Type SearchSettingsType { get; set; } = typeof(AntonioliSearchSettings);


        private readonly string _searchformat = @"https://www.antonioli.eu/en/search?utf8=✓&q={0}&gender={1}";
        private static readonly string[] Gender = { "men", "women" };

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            AntonioliSearchSettings.GenderEnum genderEnum;
            try
            {
                genderEnum = ((AntonioliSearchSettings) settings).Gender;
            }
            catch
            {
                genderEnum = AntonioliSearchSettings.GenderEnum.Both;
            }
            listOfProducts = new List<Product>();
            switch (genderEnum)
            {
                case AntonioliSearchSettings.GenderEnum.Man:
                    FindItemsForGender(listOfProducts, settings, token, "men");
                    break;
                case AntonioliSearchSettings.GenderEnum.Woman:
                    FindItemsForGender(listOfProducts, settings, token, "women");
                    break;
                default:
                    FindItemsForGender(listOfProducts, settings, token, "men");
                    FindItemsForGender(listOfProducts, settings, token, "women");
                    break;
            }


        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var page = GetWebpage(productUrl, token);
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
            name = Regex.Replace(name, @"\t|\n|\r", "");
            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private void FindItemsForGender(List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token, string gender)
        {
            string url = string.Format(_searchformat, settings.KeyWords, gender);
            var document = GetWebpage(url, token);
            HtmlNodeCollection itemCollection = document.SelectNodes("//*[@id='content']/section/article");

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

        private Product GetProduct(HtmlNode item)
        {
            try
            {
                var url = GetUrl(item);
                var name = GetName(item);
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

        private string GetName(HtmlNode item)
        {
            string brand = item.SelectSingleNode("./a/figure/figcaption/div[contains(@class, 'brand-name')]").InnerHtml;
            string category = 
                item.SelectSingleNode("./a/figure/figcaption/div[contains(@class, 'category-and-season')]/span[contains(@class, 'category')]").InnerHtml;
            string seasson =
                item.SelectSingleNode("./a/figure/figcaption/div[contains(@class, 'category-and-season')]/span[contains(@class, 'season')]").InnerHtml;
            brand = Regex.Replace(brand, @"\t|\n|\r", "");
            return $"{brand} - {category} {seasson}";
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