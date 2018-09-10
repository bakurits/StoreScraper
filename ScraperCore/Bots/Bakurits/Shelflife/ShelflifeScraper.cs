using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace StoreScraper.Bots.Bakurits.Shelflife
{
    public class ShelflifeScraper : ScraperBase
    {
        private const string SearchFormat = "http://www.shelflife.co.za/Search?search={0}";

        private const int MaxPageCount = 3;
        public override string WebsiteName { get; set; } = "Shelflife";
        public override string WebsiteBaseUrl { get; set; } = "http://www.shelflife.co.za/";
        public override bool Active { get; set; }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var itemCollection = GetProductCollection(settings, token);

            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(listOfProducts, settings, item);
            }
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);

            var name = document.SelectSingleNode("//div[contains(@class, 'product_info')]/h1").InnerHtml;
            var image = WebsiteBaseUrl +
                        document.SelectSingleNode("//div[@id='large_img']/img").GetAttributeValue("src", "");
            var priceNode = document.SelectSingleNode("//div[contains(@class, 'price')]").InnerHtml;
            var price = Utils.ParsePrice(priceNode);
            var details = new ProductDetails
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };

            var node = document.SelectSingleNode("//*[@id='addToCart']/div/div/div/select[@id = 'size']");

            var sizeCollection = node.SelectNodes("./option");

            foreach (var size in sizeCollection)
            {
                var sz = size.GetAttributeValue("value", "");
                if (sz.Length > 0) details.AddSize(sz, "Unknown");
            }

            return details;
        }

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            const string searchFormat = "https://www.shelflife.co.za/New-arrivals?page={0}";
            var urls = new List<string>();
            for (var i = 1; i <= MaxPageCount; i++)
            {
                var url = string.Format(searchFormat, i);
                urls.Add(url);
            }

            var pages = GetPageTask(urls, token).Result;
            foreach (var page in pages)
            {
                var items = page.SelectNodes("//body/div/div/div[contains(@class, 'col-xs-6 col-sm-3')]");
                if (items == null) break;
                foreach (var item in items)
                {
                    token.ThrowIfCancellationRequested();
                    LoadSingleProduct(listOfProducts, null, item);
                }
            }
        }

        private static async Task<List<HtmlNode>> GetPageTask(List<string> urls, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);

            var documents = await Task.WhenAll(urls.Select(i => client.GetDocTask(i, token)));
            return documents.Select(document => document.DocumentNode).ToList();
        }

        private static HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private static HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            var url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            return document.SelectNodes("//body/div/div/div[contains(@class, 'col-xs-6 col-sm-3')]");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            var name = GetName(item);
            var url = GetUrl(item);
            var price = GetPrice(item);
            var imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "R");
            if (settings == null || Utils.SatisfiesCriteria(product, settings))
                listOfProducts.Add(product);
        }

        private static string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./a/div/div/div[contains(@class, 'title')]").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            var url = item.SelectSingleNode("./a").GetAttributeValue("href", "");
            return WebsiteBaseUrl + url;
        }

        private static double GetPrice(HtmlNode item)
        {
            var priceContainer = item.SelectSingleNode("./a/div/div/div[contains(@class, 'price')]").InnerHtml
                .Substring(1);
            var ind = priceContainer.IndexOf("<span>", StringComparison.Ordinal);
            if (ind != -1) priceContainer = priceContainer.Substring(0, ind);
            double.TryParse(priceContainer, out var ans);
            return ans;
        }

        private string GetImageUrl(HtmlNode item)
        {
            var url = item.SelectSingleNode("./a/div/img").GetAttributeValue("src", "");
            return WebsiteBaseUrl + url;
        }
    }
}