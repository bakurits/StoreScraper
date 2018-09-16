using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Jordan.VooBerlin
{
    public class VooScraper: ScraperBase
    {
        public override string WebsiteName { get; set; } = "VooBerlin";
        public override string WebsiteBaseUrl { get; set; } = "https://vooberlin.com/";
        public override bool Active { get; set; }
        
        private const string SearchUrl = @"https://www.vooberlin.com/search?sSearch={0}&p={1}";

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            listOfProducts = new List<Product>();
        }

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
            double? price = GetPrice(item);
            string imgurl = GetImg(item);
            if (price != null)
            {
                var product = new Product(this, name, url, (double)price, imgurl, url, "EUR");
                if (Utils.SatisfiesCriteria(product, settings))
                {
                    listOfProducts.Add(product);
                }
            }
        }

        private string GetImg(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div[2]/a/span/span/img").GetAttributeValue("srcset", null);
        }

        private double ParsePrice(string pricee)
        {
            
            string result = Regex.Match(pricee, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }

        private double? GetPrice(HtmlNode item)
        {
            try
            {
                return ParsePrice(item.SelectSingleNode("./div[1]/div[2]/div[1]/div/div/div/span").InnerHtml);
            }
            catch
            {
                return null;
            }

        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./div[1]/div[2]/div[1]/span/a").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div[2]/a").GetAttributeValue("href", null);
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

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to basketrevolution website");
                throw new WebException("Can't connect to website");
            }

            var product = document.SelectSingleNode("//div[contains(@class, 'product--detail-upper')]");
            var imgurl = product
                .SelectSingleNode(
                    "//div[contains(@class,'image--box')][1]/span[1]/span[@class='image--media'][1]/img[1]")
                .GetAttributeValue("srcset", null);
            var name = product.SelectSingleNode("//h1[@class='product--title'][1]/span[1]").InnerHtml;
            var price = Utils.ParsePrice(product.SelectSingleNode("//div[@class='w_price-infos'][1]").InnerHtml);
            var sizes = product.SelectSingleNode("//select[contains(@class,'w_select-item')][1]");
            var sizesList = sizes.SelectNodes("./option");
            
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