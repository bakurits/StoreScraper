using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Jordan.Byparra
{
    public class ByparraScraper: ScraperBase
    {
        public override string WebsiteName { get; set; } = "Byparra";
        public override string WebsiteBaseUrl { get; set; } = "https://byparra.com";
        public override bool Active { get; set; }
        
        private const string SearchUrl = @"https://byparra.com/?q={0}";
        
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();

            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
          
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(listOfProducts, settings, item, token);
            }

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item, CancellationToken token)
        {
           
            string name = GetName(item);
            string url = GetUrl(item);
            double price = GetPrice(url, token);
            string imgurl = GetImg(item);
            var product = new Product(this, name, url, price, imgurl, url, "EUR");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
         
        }

        private string GetImg(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }

        private double ParsePrice(string pricee)
        {
            
            string result = Regex.Match(pricee, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }

        private double GetPrice(string url, CancellationToken token)
        {
            var urlNew = "https://byparra.com" + url.Substring(1);
            var resp = GetWebpage(urlNew, token);    
            var price = resp.SelectSingleNode("//p[contains(@class, 'price')]/b").InnerHtml;
            return ParsePrice(price);

        }
        
        private string GetName(HtmlNode item)
        {
            return item.GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.GetAttributeValue("href", null);
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;
        }


        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            var toSearch = string.Format(SearchUrl, settings.KeyWords);
            var searchResults = GetWebpage(toSearch, token);
            return searchResults.SelectNodes("//a[contains(@class, 'product')]");
            
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var resp = GetWebpage(productUrl, token);
            if (resp == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to basketrevolution website");
                throw new WebException("Can't connect to website");
            }

            var product = resp.SelectSingleNode("//div[contains(@class, 'row col-wrap')]");
            var name = product.SelectSingleNode("//h2[1]").InnerHtml;
            var price = Utils.ParsePrice(product.SelectSingleNode("//p[contains(@class, 'price')][1]/b").InnerHtml);
            var imgurl = "https://byparra.com" + product
                             .SelectSingleNode("//div[@class='item active'][1]/img[@class='img-responsive'][1]")
                             .GetAttributeValue("src", null);
            var sizes = product.SelectSingleNode("//select[contains(@class,'form-control')][1]");
            var sizesList = sizes.SelectNodes("//option");
            
            ProductDetails result = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = imgurl,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };
            
            foreach (var size in sizesList)
            {
                var sizeString = size.InnerHtml;

                if (sizeString != "size")
                {
                    result.AddSize(sizeString, "Unknown");
                }
            }
            
            return result;
        }
    }
}