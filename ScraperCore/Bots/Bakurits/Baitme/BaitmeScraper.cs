using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Bakurits.Baitme
{
    public class BaitmeScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Bait";
        public override string WebsiteBaseUrl { get; set; } = "http://www.baitme.com/";
        public override bool Active { get; set; }

        private readonly string _urlFormat = @"http://www.baitme.com/catalogsearch/result/?q={0}";
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
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
            var image = page.SelectSingleNode("//div[contains(@class, 'product-image')]/img").GetAttributeValue("src", "");
            var priceNode = page.SelectSingleNode("//div[contains(@class, 'product-shop')]");
            ProductDetails details = new ProductDetails()
            {
                Price = GetPrice(priceNode),
                Name = name,
                Currency = "USD",
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
                var productCount = (JArray)sz.SelectToken("products");
                if (productCount.Count > 0)
                    details.AddSize(sizeName, "Unknown");
            }
            return details;
        }

        private readonly List<string> _newArrivalPageUrls = new List<string>
        {
            "http://www.baitme.com/bait-products",
            "http://www.baitme.com/nike",
            "http://www.baitme.com/footwear",
            "http://www.baitme.com/headwear",
            "http://www.baitme.com/apparel",
            "http://www.baitme.com/accessories",
            "http://www.baitme.com/collectibles",
            "http://www.baitme.com/skateboard-snowboard"
        };
        
        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
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
            HtmlNodeCollection collection = page.SelectNodes("//ul[contains(@class, 'products-grid')]/li[contains(@class, 'item last')]");

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
            var name = item.SelectSingleNode("./a").GetAttributeValue("title", null).EscapeNewLines();
            return name;
        }
        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null).Substring(0);
        }
        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null).Substring(0);
        }
        private double GetPrice(HtmlNode item)
        {
            var priceBox = item.SelectSingleNode("./div/div[contains(@class, 'price-box')]");
            var specialPrices = priceBox.SelectNodes("./p[contains(@class, 'special-price')]");
            double result = 0;
            if (specialPrices != null)
            {
                foreach (var price in specialPrices)
                {
                    string curPrice = GetInsidePrice(price);
                   
                    curPrice = Regex.Replace(curPrice, "[^0-9.]", "");

                    if (curPrice.Length > 0 && double.TryParse(curPrice, out result))
                    {
                        return result;
                    }
                        
                   
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

        private string GetInsidePrice(HtmlNode item)
        {
            return item.SelectSingleNode("./span[@class = 'price']").InnerHtml;
        }
        private string GetCurrency(HtmlNode item)
        { 
            return "USD";
        }

        private JToken GetSizesToken(JObject token)
        {
            var sizes = token.SelectToken("attributes");
            foreach (var item in sizes)
            {
                var attPath = item.Path;
                attPath = attPath.Substring(attPath.LastIndexOf(".", StringComparison.Ordinal) + 1);
                if (int.TryParse(attPath, out var res))
                {
                    return item.First;
                }
            }

            return null;
        }
    }
}