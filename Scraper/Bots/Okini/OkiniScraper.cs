using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Core;
using StoreScraper.Models;
using System.Text.RegularExpressions;
using System;

namespace StoreScraper.Bots.Okini
{
    public class OkiniScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Okini";
        public override string WebsiteBaseUrl { get; set; } = "https://row.oki-ni.com";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://row.oki-ni.com/catalogsearch/result/?q=sneaker&product_list_order=created_at";
        private const string priceRegex = "class=\"price\">\\$(\\d+(\\.\\d+)?)<";
        private const string sizesRegex = "\"label\":\"(.*?)\",\"products\":\\[\"(\\d+)\"\\]";

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
                if (!details.SizesList.Exists(size => size.Key == sz) && sz.Length > 0 && !(sz.Contains("{")))
                {
                    details.AddSize(sz, "Unknown");
                }
                match = match.NextMatch();
            }
            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return client.GetDoc(url, token).DocumentNode;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            return document.SelectNodes("//li[@class='item product product-item']");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "USD");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a").GetAttributeValue("href", null);
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
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }
    }
}
