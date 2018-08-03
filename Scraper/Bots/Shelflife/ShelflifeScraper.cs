using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Browser;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Shelflife
{
    public class ShelflifeScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Shelflife";
        public override string WebsiteBaseUrl { get; set; } = "https://www.shelflife.co.za/";
        public override bool Active { get; set; }

        private const string SearchFormat = "https://www.shelflife.co.za/Search?search={0}";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
            
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(listOfProducts, settings, item);
            }

        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var document = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();

            var node = document.SelectSingleNode("//*[@id='addToCart']/div/div/div/select[@id = 'size']");

            var sizeCollection = node.SelectNodes("./option");

            foreach (var size in sizeCollection)
            {
                string sz = size.GetAttributeValue("value", "");
                if (sz.Length > 0)
                {
                    details.SizesList.Add(sz);
                }
                
            }

            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            using (var client = ClientFactory.GetProxiedClient(autoCookies: true).AddHeaders(ClientFactory.FireFoxHeaders))
            {
                var document = client.GetDoc(url, token).DocumentNode;
                return client.GetDoc(url, token).DocumentNode;
            }
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            return document.SelectNodes("//body/div/div/div[contains(@class, 'col-xs-6 col-sm-3')]");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item);
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            listOfProducts.Add(new Product(this, name, url, price, url, imageUrl, "R"));
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./a/div/div/div[contains(@class, 'title')]").InnerHtml;
        }
        private string GetUrl(HtmlNode item)
        {
            string url = item.SelectSingleNode("./a").GetAttributeValue("href", "");
            return WebsiteBaseUrl + url;
        }
        private double GetPrice(HtmlNode item)
        {
            string priceContainer = item.SelectSingleNode("./a/div/div/div[contains(@class, 'price')]").InnerHtml.Substring(1);
            int ind = priceContainer.IndexOf("<span>", StringComparison.Ordinal);
            if (ind != -1) priceContainer = priceContainer.Substring(0, ind);
            double.TryParse(priceContainer, out var ans);
            return ans;
        }
        private string GetImageUrl(HtmlNode item)
        {
            string url = item.SelectSingleNode("./a/div/img").GetAttributeValue("src", "");
            return WebsiteBaseUrl + url;
        }
    }
}