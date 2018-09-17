using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Jordan.BounceWear
{
    public class BounceWearScrape: ScraperBase
    {
        public override string WebsiteName { get; set; } = "BounceWear";
        public override string WebsiteBaseUrl { get; set; } = "https://bouncewear.com/";
        public override bool Active { get; set; }
        
        private const string SearchUrl = "https://bouncewear.com/search";

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl = "https://bouncewear.com/category/schoenen";
            ScrapNewArrivals(listOfProducts, token, searchUrl);
            searchUrl = "https://bouncewear.com/category/nba";
            ScrapNewArrivals(listOfProducts, token, searchUrl);
        }

        private void ScrapNewArrivals(List<Product> listOfProducts, CancellationToken token, string searchUrl)
        {
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            var searchResults = document.DocumentNode;
            HtmlNodeCollection itemCollection = searchResults.SelectNodes("//div[contains(@class, 'productBox')]");
            if (itemCollection == null)
                return;
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(listOfProducts, null, item);
            }

        }
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
          
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
            if (itemCollection == null)
                return;
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(listOfProducts, settings, item);
            }

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
           
            string name = GetName(item);
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imgurl = GetImg(item);
            var product = new Product(this, name, url, price, imgurl, url, "EUR");
            if (settings == null)
            {
                listOfProducts.Add(product);
                return;
            }
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private string GetImg(HtmlNode item)
        {
            return item.SelectSingleNode(".//div[contains(@class, 'productImage')]/a/img").GetAttributeValue("src", null);
        }

        private double GetPrice(HtmlNode item)
        {
            var fullPrice = item.SelectSingleNode(".//div[contains(@class, 'productCaption')]/h3").InnerHtml;
            string result = Regex.Match(fullPrice, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode(".//div[contains(@class, 'productCaption')]/a/h5").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//div[contains(@class, 'productImage')]/a").GetAttributeValue("href", null);
        }

        private HtmlNode PostSearch(string url, CancellationToken token, SearchSettingsBase settings)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(null, true);
            var searchToken = GetSearchToken(client, token);
            var keyword = settings.KeyWords;
            
            var values = new Dictionary<string, string>
            {
                {"_token", searchToken },
                {"searchstring", keyword},
            };

            var postParams = new FormUrlEncodedContent(values);
            
            var x = client.PostDoc(url, token, postParams);
            return x.DocumentNode;
        }

        private string GetSearchToken(HttpClient client, CancellationToken token)
        {
            
            var document = client.GetDoc(WebsiteBaseUrl, token).DocumentNode;
            var _token = document.SelectSingleNode("//input[contains(@name, '_token')]").Attributes["value"].Value;
            return _token;

        }
        
        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            var searchResults = PostSearch(SearchUrl, token, settings);
            return searchResults.SelectNodes("//div[contains(@class, 'productBox')]");
            
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to basketrevolution website");
                throw new WebException("Can't connect to website");
            }

            var product = document.SelectSingleNode("//div[contains(@class, 'media flex-wrap')]");
            var name = product.SelectSingleNode("//h2[1]").InnerHtml;
            var price = Utils.ParsePrice(product.SelectSingleNode("//h1[@class='mb-3'][1]/strong").InnerHtml);
            var imgurl = product.SelectSingleNode("//div[@class='thumb'][1]/img[1]").GetAttributeValue("src", null);
            var sizes = product.SelectSingleNode("//select[@id='variation_id'][1]");
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
                
                result.AddSize(sizeString, "Unknown");
            }
            
            return result;


        }
    }
}