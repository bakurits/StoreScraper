using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Core;
using StoreScraper.Models;
using System.Text.RegularExpressions;
using System;

namespace StoreScraper.Bots.Endclothing
{
    public class EndclothingScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Endclothing";
        public override string WebsiteBaseUrl { get; set; } = "https://www.endclothing.com";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.endclothing.com/us/catalogsearch/result/?q={0}";
        private const string priceRegex = "<span class=\"price\">\\$(\\d+)</span>";
        private const string sizesRegex = "\"label\":\"UK (\\d+(\\.\\d+)?)\",\"products\"";
        private const string idRegex = "data-product-id=\"(\\d+)\"";

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

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var document = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();

            Match match = Regex.Match(document.InnerHtml, sizesRegex);

            while (match.Success)
            {
                var sz = match.Groups[1].Value;
                if (!details.SizesList.Contains(sz) && sz.Length > 0)
                {
                    details.SizesList.Add(sz);
                }
                match = match.NextMatch();
            }
            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient();
            var document = client.GetDoc(url, token).DocumentNode;
            return client.GetDoc(url, token).DocumentNode;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            return document.SelectNodes("//div[contains(@class, 'item product product-item')]");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "EUR");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-item-info']/a/div/img[1]").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-item-info']/a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            Match match = Regex.Match(item.InnerHtml, priceRegex);
            double price = -1;
            while (match.Success)
            {
                price = Convert.ToDouble(match.Groups[1].Value);
                match = match.NextMatch();
            }
            return price;
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-item-info']/a/div/img[1]").GetAttributeValue("src", null);
        }
    }
}
