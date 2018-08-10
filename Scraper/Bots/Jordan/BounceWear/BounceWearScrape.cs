using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
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
        
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
          
            foreach (var item in itemCollection)
            {
                //token.ThrowIfCancellationRequested();
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
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private string GetImg(HtmlNode item)
        {
            return item.SelectSingleNode(".//div[1]/a/img").GetAttributeValue("src", null);
        }

        private double GetPrice(HtmlNode item)
        {
            var fullPrice = item.SelectSingleNode(".//div[2]/h3").InnerHtml;
            string result = Regex.Match(fullPrice, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode(".//div[2]/a/h5").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//div[1]/a").GetAttributeValue("href", null);
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


        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            var searchResults = PostSearch(SearchUrl, token, settings);
            return searchResults.SelectNodes("//div[contains(@class, 'productBox')]");
            
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}