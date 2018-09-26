using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Bakurits.Baitme
{
    public class BaitmeScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Bait";
        public override string WebsiteBaseUrl { get; set; } = "https://www.baitme.com/";
        public override bool Active { get; set; }

        private readonly string _urlFormat = @"https://www.baitme.com/catalogsearch/result/?q={0}";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            GetProductsForPage(_urlFormat, data, settings, token).Wait(token);

            listOfProducts = new List<Product>(data.Keys);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var page = GetWebpage(productUrl, token);
            var nameContainer = page.SelectSingleNode("//div[contains(@class, 'product-name')]/span");
            var name = nameContainer.InnerHtml.EscapeNewLines();
            var image = page.SelectSingleNode("//div[contains(@class, 'product-image')]/img")
                .GetAttributeValue("src", "");
            var priceNode = page.SelectSingleNode("//div[contains(@class, 'product-shop')]");
            ProductDetails details = new ProductDetails()
            {
                Price = GetPrice(priceNode),
                Name = name,
                Currency = "$",
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };

            //product.ImageUrl = page.SelectSingleNode("//img[@id = 'image-main']").GetAttributeValue("src", null);

            var jsonStr = Regex.Match(page.InnerHtml, @"var spConfig = new Product.Config\((.*)\)").Groups[1].Value;
            JObject parsed = JObject.Parse(jsonStr);

            var sizes = GetSizesToken(parsed);
            sizes = sizes.SelectToken("options");


            foreach (JToken sz in sizes.Children())
            {
                var sizeName = (string) sz.SelectToken("label");
                var productCount = (JArray) sz.SelectToken("products");
                if (productCount.Count > 0)
                    details.AddSize(sizeName, "Unknown");
            }

            return details;
        }

        private readonly List<string> _newArrivalPageUrls = new List<string>
        {
            "https://www.baitme.com/bait-products",
            "https://www.baitme.com/nike",
            "https://www.baitme.com/footwear",
            "https://www.baitme.com/headwear",
            "https://www.baitme.com/apparel",
            "https://www.baitme.com/accessories",
            "https://www.baitme.com/collectibles",
            "https://www.baitme.com/skateboard-snowboard"
        };

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            Task.WhenAll(_newArrivalPageUrls.Select(url => GetProductsForPage(url, data, null, token))).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }

        private async Task GetProductsForPage(string url, ConcurrentDictionary<Product, byte> data,
            SearchSettingsBase settings, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var page = (await client.GetDocTask(url, token)).DocumentNode;
            HtmlNodeCollection collection =
                page.SelectNodes("//ul[contains(@class, 'products-grid')]/li[contains(@class, 'item last')]");

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

        private static HtmlNode GetWebpage(string url, CancellationToken token)
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

        private static string GetName(HtmlNode item)
        {
            var name = item.SelectSingleNode("./a").GetAttributeValue("title", null).EscapeNewLines();
            return name;
        }

        private static string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null).Substring(0);
        }

        private static string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null).Substring(0);
        }

        private static double GetPrice(HtmlNode item)
        {
            var priceBox = item.SelectSingleNode("./div/div[contains(@class, 'price-box')]");
            var specialPrices = priceBox.SelectNodes("./p[contains(@class, 'special-price')]");
            double result = 0;
            if (specialPrices != null)
            {
                if (specialPrices.Select(GetInsidePrice).Select(curPrice => Regex.Replace(curPrice, "[^0-9.]", "")).Any(
                    curPrice =>
                        curPrice.Length > 0 && double.TryParse(curPrice, out result)))
                {
                    return result;
                }
            }
            else
            {
                var price = priceBox.SelectSingleNode("./span[contains(@class, 'regular-price')]");
                string curPrice = GetInsidePrice(price);
                curPrice = Regex.Replace(curPrice, "[^0-9.]", "");
                double.TryParse(curPrice, out result);
            }

            return result;
        }

        private static string GetInsidePrice(HtmlNode item)
        {
            return item.SelectSingleNode("./span[@class = 'price']").InnerHtml;
        }

        private static string GetCurrency(HtmlNode item)
        {
            return "$";
        }

        private static JToken GetSizesToken(JToken token)
        {
            var sizes = token.SelectToken("attributes");
            foreach (var item in sizes)
            {
                var attPath = item.Path;
                attPath = attPath.Substring(attPath.LastIndexOf(".", StringComparison.Ordinal) + 1);
                if (int.TryParse(attPath, out _))
                {
                    return item.First;
                }
            }

            return null;
        }
    }
}