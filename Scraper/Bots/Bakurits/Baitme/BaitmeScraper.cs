using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Factory;
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
            listOfProducts = new List<Product>();
            var page = GetWebpage(string.Format(_urlFormat, settings.KeyWords), token);
            
            HtmlNodeCollection collection = page.SelectNodes("//ul[contains(@class, 'products-grid')]/li[contains(@class, 'item last')]");

            foreach (var item in collection)
            {
                token.ThrowIfCancellationRequested();
                Product product = GetProduct(item);
                if (product != null && Utils.SatisfiesCriteria(product, settings))
                {
                    listOfProducts.Add(product);
                }
            }


        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var page = GetWebpage(productUrl, token);
            ProductDetails details = new ProductDetails();

            //product.ImageUrl = page.SelectSingleNode("//img[@id = 'image-main']").GetAttributeValue("src", null);

            var jsonStr = Regex.Match(page.InnerHtml, @"var spConfig = new Product.Config\((.*)\)").Groups[1].Value;
            JObject parsed = JObject.Parse(jsonStr);

            var sizes = parsed.SelectToken("attributes").SelectToken("188").SelectToken("options");
            foreach (JToken sz in sizes.Children())
            {
                var sizeName = (string) sz.SelectToken("label");
                details.AddSize(sizeName, "Unknown");
            }
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
            var name = item.SelectSingleNode("./a").GetAttributeValue("title", null);
            name = Regex.Replace(name, @"\t|\n|\r", "");
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
    }
}