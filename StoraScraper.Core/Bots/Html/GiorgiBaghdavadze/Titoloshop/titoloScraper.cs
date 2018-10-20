using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.GiorgiBaghdavadze.Titoloshop
{
    public  class TitoloScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Titoloshop";
        public override string WebsiteBaseUrl { get; set; } = "https://en.titoloshop.com";
        public override bool Active { get; set; }


        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            var searchUrl = "https://en.titolo.ch/brands";
            listOfProducts = new List<Product>();
            scrap(listOfProducts, searchUrl, token, null);
        }


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                $"https://en.titoloshop.com/catalogsearch/result/index/?dir=desc&order=created_at&q={settings.KeyWords}";
            scrap(listOfProducts, searchUrl, token, settings);
        }


        private void scrap(List<Product> listOfProducts, String searchUrl, CancellationToken token, SearchSettingsBase settings)
        {
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            Logger.Instance.WriteErrorLog("Unexpected html!");
            var nodes = document.DocumentNode.SelectSingleNode("//ul[contains(@class, 'no-bullet') and contains(@class, 'small-block-grid-2')]");
            if (nodes == null)
            {
                return;
            }
            var children = nodes.SelectNodes("./li");
            if (children == null)
            {
                return;
            }

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child,settings);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, child, settings);
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
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child,SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child,settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

       
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            string imageURL = child.SelectSingleNode(".//a[contains(@class, 'product-image')]/img")?.GetAttributeValue("src", null);
            string productName = child.SelectSingleNode(".//span[contains(@class,'name')]").InnerText;
            string productURL = child.SelectSingleNode(".//a[contains(@class,'product-name')]").GetAttributeValue("href", null);
            string priceIntoString = child.SelectSingleNode(".//span[@class='price']")?.InnerText;
            if (priceIntoString == null)
            {
                return;
            }
            double price = getPrice(priceIntoString);
            var product = new Product(this, productName, productURL, price, imageURL, productURL);
            if (settings == null)
                listOfProducts.Add(product);
            else
                if (Utils.SatisfiesCriteria(product, settings))
                {
                    listOfProducts.Add(product);
                }

        }

        private double getPrice(string priceIntoString)
        {
            Debug.Print(priceIntoString);
            string result = Regex.Match(priceIntoString, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            const string xPath = "//*[@id='attributesize-size_eu']/option";
            string name = document.SelectSingleNode("//h1[contains(@class,'product-name')]/strong").InnerText;
            
            string priceIntoString = document.SelectSingleNode("//span[@class='price'][last()]").InnerText;
            string result = Regex.Match(priceIntoString, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

            string imageURL = document.SelectSingleNode("//img[@id='image']").GetAttributeValue("src",null);
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

            var nodes = document.SelectNodes(xPath);
            if (nodes == null)
            {
                return details;
            }

            var sizes = nodes.Select(node => node.InnerText.Trim()).Where(element => !element.Contains("Choose"));
            foreach (var size in sizes)
            {
                details.AddSize(size, "Unknown");
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
