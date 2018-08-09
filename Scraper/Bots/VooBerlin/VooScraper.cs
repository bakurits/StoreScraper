using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.VooBerlin
{
    public class VooScraper: ScraperBase
    {
        public override string WebsiteName { get; set; } = "VooBerlin";
        public override string WebsiteBaseUrl { get; set; } = "https://vooberlin.com/";
        public override bool Active { get; set; }
        
        private const string SearchUrl = @"https://www.vooberlin.com/search?sSearch={0}&p={1}";
        
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var pageOne = GetWebpage(String.Format(SearchUrl, settings.KeyWords, "1"), token);
            
            var firstResults = pageOne.SelectNodes("//div[contains(@class, 'product--box')]");
            foreach (var item in firstResults)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(listOfProducts, settings, item, token);
            }

            var count = pageOne.SelectSingleNode("//span[contains(@class, 'headline--product-count')]").InnerHtml;
            var scrapeAmount = Math.Ceiling(decimal.Parse(count) / 12) -1 ;
            IEnumerable<int> toScrape = Enumerable.Range(2, (int)scrapeAmount);

            foreach (int num in toScrape)
            {
                var page = GetWebpage(String.Format(SearchUrl, settings.KeyWords, num.ToString()), token);
                var results =  page.SelectNodes("//div[contains(@class, 'product--box')]");
                foreach (var item in results)
                {
                    token.ThrowIfCancellationRequested();
                    LoadSingleProduct(listOfProducts, settings, item, token);
                }
                
            }
            
            
         

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item, CancellationToken token)
        {
           
            string name = GetName(item);
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imgurl = GetImg(item);
            var product = new Product(this, name, url, price, imgurl, url, "EUR");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
         
        }

        private string GetImg(HtmlNode item)
        {
            return item.SelectSingleNode("//img[1]").GetAttributeValue("srcset", null);
        }

        private double ParsePrice(string pricee)
        {
            
            string result = Regex.Match(pricee, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }

        private double GetPrice(HtmlNode item)
        {
            return ParsePrice(item.SelectSingleNode("//span[@class='price--discount is--nowrap'][1]").InnerHtml);

        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("//a[@class='product--title'][1]").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("//a[@class='product--image'][1]").GetAttributeValue("href", null);
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;
        }


        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            var toSearch = String.Format(SearchUrl, settings.KeyWords);
            var searchResults = GetWebpage(toSearch, token);
            return searchResults.SelectNodes("//a[contains(@class, 'product')]");
            
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}