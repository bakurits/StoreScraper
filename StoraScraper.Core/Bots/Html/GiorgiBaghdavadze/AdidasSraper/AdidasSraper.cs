using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.GiorgiBaghdavadze.Adidas
{
    public class AdidasSraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Adidas";
        public override string WebsiteBaseUrl { get; set; } = "https://www.adidas.com";
        public override bool Active { get; set; }


        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            var searchUrl = "https://www.adidas.com/us/new_arrivals?grid=true&sort=newest-to-oldest";
            listOfProducts = new List<Product>();
            scrap(listOfProducts, searchUrl, token, null);
        }


        private void scrap(List<Product> listOfProducts, string searchUrl, CancellationToken token, SearchSettingsBase settings)
        {
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            var ds = document.DocumentNode;
            var nodes = ds.SelectNodes("//div[contains(@data-auto-id, 'product_container')]/div");
            foreach (var child in nodes)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child, settings);
#else
                LoadSingleProductTryCatchWraper(listOfProducts,child,settings);
#endif
            }
        }


        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="info"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child, settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private string getProductUrl(HtmlNode child)
        {
            return this.WebsiteBaseUrl + child.SelectSingleNode(".//div[contains(@class, 'gl-product-card__media')]/a")?.GetAttributeValue("href", null);
        }

        private string getImageUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//img[contains(@class, 'gl-product-card__image')]")?.GetAttributeValue("src", null);
        }

        private string getProductName(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class, 'gl-product-card__name')]")?.GetAttributeValue("title", null);
        }

        private double getPrice(HtmlNode child)
        {
            string priceIntoString = child.SelectSingleNode(".//div[contains(@class,'gl-price-container')]/span").InnerText;
            Debug.Print(priceIntoString);
            string result = Regex.Match(priceIntoString, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }

        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            var r = child.InnerText;
            string imageURL = getImageUrl(child);
            string productURL = getProductUrl(child);
            string productName = getProductName(child);
            if (imageURL == null || productName == null || productURL == null) return;
            double price = getPrice(child);
            var product = new Product(this, productName, productURL, price, imageURL, productURL);
            if (settings == null)
                listOfProducts.Add(product);
            else
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            HtmlNode ds = GetWebpage(productUrl, token);
            string name = ds.SelectSingleNode("//h1[contains(@data-auto-id,'product-title')]").InnerText;
            string priceIntoString = ds.SelectSingleNode("//div[contains(@class,'gl-price-container')]/span").InnerText;
            string result = Regex.Match(priceIntoString, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            string imageURL = ds.SelectSingleNode("//img[@class='performance-item']").GetAttributeValue("src", null);
            int t = productUrl.LastIndexOf("/", StringComparison.Ordinal);
            int r = productUrl.LastIndexOf(".", StringComparison.Ordinal);

            var sku = productUrl.Substring(t + 1, r - t - 1);
            ProductDetails details = new ProductDetails()
            {
                Name = name,
                Price = price,
                ImageUrl = imageURL,
                Url = productUrl,
                Id = productUrl,
                Currency = "CHF",
                ScrapedBy = this
            };

            string search = $"https://www.adidas.com/api/products/{sku}/availability?sitePath=us";
            ds = GetWebpage(search, token);
            string toJason = ds.InnerText;
            int arrayStart = toJason.LastIndexOf("[", StringComparison.Ordinal);
            int arrayEnd = toJason.LastIndexOf("}");
            if (arrayStart == -1) return details;
            toJason = toJason.Substring(arrayStart, arrayEnd - arrayStart);
            JArray parsed = null; 
            try
            {
                parsed = JArray.Parse(toJason);
            }
            catch (Exception e)
            {
                return details;
            }
            foreach (var x in parsed.Children())
            {
                var value = (string)x.SelectToken("size");
                var availability_status = (string)x.SelectToken("availability_status");
                if (availability_status == "IN_STOCK")
                    details.AddSize(value, "Unknown");
            }

            return details;
        }
        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;

        }

    }
}

//"\"variation_list\":[{\"sku\":\"B37458_530\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"5\"},{\"sku\":\"B37458_540\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"5.5\"},{\"sku\":\"B37458_550\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"6\"},{\"sku\":\"B37458_560\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"6.5\"},{\"sku\":\"B37458_570\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"7\"},{\"sku\":\"B37458_580\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"7.5\"},{\"sku\":\"B37458_590\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"8\"},{\"sku\":\"B37458_600\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"8.5\"},{\"sku\":\"B37458_610\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"9\"},{\"sku\":\"B37458_620\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"9.5\"},{\"sku\":\"B37458_630\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"10\"},{\"sku\":\"B37458_640\",\"availability\":15,\"availability_status\":\"IN_STOCK\",\"size\":\"10.5\"},{\"sku\":\"B37458_650\",\"availability\":0,\"availability_status\":\"NOT_AVAILABLE\",\"size\":\"11\"}]"