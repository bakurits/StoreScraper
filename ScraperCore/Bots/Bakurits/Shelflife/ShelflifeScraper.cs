using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Bakurits.Shelflife
{
    public class ShelflifeScraper : ScraperBase
    {
        private const string SearchFormat = "https://www.shelflife.co.za/Search?search={0}";
        public override string WebsiteName { get; set; } = "Shelflife";
        public override string WebsiteBaseUrl { get; set; } = "https://www.shelflife.co.za/";
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
            var image = WebsiteBaseUrl + document.SelectSingleNode("//div[@id='large_img']/img").GetAttributeValue("src", "");
            var priceNode = document.SelectSingleNode("//div[contains(@class, 'price')]").InnerHtml;
            Price price = Utils.ParsePrice(priceNode);
            ProductDetails details = new ProductDetails()
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
                if (sz.Length > 0) details.AddSize(sz,"Unknown");
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
            Product product = new Product(this, name, url, price, imageUrl, url, "R");
            if (Utils.SatisfiesCriteria(product, settings))
                listOfProducts.Add(product);
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./a/div/div/div[contains(@class, 'title')]").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            var url = item.SelectSingleNode("./a").GetAttributeValue("href", "");
            return WebsiteBaseUrl + url;
        }

        private double GetPrice(HtmlNode item)
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