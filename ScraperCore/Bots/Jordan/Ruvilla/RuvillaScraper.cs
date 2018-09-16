
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Jordan.Ruvilla
{
    public class RuvillaScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Ruvilla";
        public override string WebsiteBaseUrl { get; set; } = "https://ruvilla.com/";
        public override bool Active { get; set; }

        private const string SearchUrl = "https://www.ruvilla.com/catalogsearch/result/?q={0}";

        private readonly List<string> _newArrivalPageUrls = new List<String>
        {
            "https://www.ruvilla.com/men/footwear/new.html",
            "https://www.ruvilla.com/men/apparel/new.html",
            "https://www.ruvilla.com/men/accessories/new.html",
            "https://www.ruvilla.com/women/footwear/new.html",
            "https://www.ruvilla.com/women/apparel/new.html",
            "https://www.ruvilla.com/kids/boys-footwear/new.html",
            "https://www.ruvilla.com/kids/girls-footwear/new.html",
            "https://www.ruvilla.com/kids/apparel/new.html"
        };

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            Task.WhenAll(_newArrivalPageUrls.Select(url => Scrap(url, data, null, token))).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }

        private async Task Scrap(string url, ConcurrentDictionary<Product, byte> data, SearchSettingsBase settings, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            GetWebpage(client, WebsiteBaseUrl, token);
            var rooturl = url;
            var document =(await client.GetDocTask(rooturl,token));
            var rootSearch = document.DocumentNode;

            var initialProducts = rootSearch.SelectNodes("//div[@class='product']");
            var results = rootSearch.SelectSingleNode("//h1").InnerHtml;
            var count = decimal.Parse(results.Split('(', ')')[1]);
            var scrapeAmount = Math.Ceiling(count / 12) - 1;

            foreach (var item in initialProducts)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(data, settings, item);
            }

            IEnumerable<int> toScrape = Enumerable.Range(2, (int)scrapeAmount);

            foreach (int num in toScrape)
            {
                rooturl += "&p=" + num.ToString();
                var newSearch = GetWebpage(client, rooturl, token);
                var newProducts = newSearch.SelectNodes("//div[@class='product']");
                foreach (var item in newProducts)
                {
                    token.ThrowIfCancellationRequested();
                    LoadSingleProduct(data, settings, item);
                }
            }
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            var data = new ConcurrentDictionary<Product,byte>();
            var rooturl = string.Format(SearchUrl, settings.KeyWords);
            Scrap(rooturl,data,settings,token).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }

        private void LoadSingleProduct(ConcurrentDictionary<Product,byte> data, SearchSettingsBase settings, HtmlNode item)
        {
           
            string name = GetName(item);
            string url = GetUrl(item);
            var pricee = GetPrice(item);
            string imgurl = GetImg(item);
            if (pricee == null)
            {
                return;
            }
           
            if(pricee != "SEE CART FOR PRICE")
            {
                Debug.WriteLine(pricee);
                var price = Utils.ParsePrice(pricee);

                var product = new Product(this, name, url, price.Value, imgurl, url, price.Currency);
                if (Utils.SatisfiesCriteria(product, settings))
                {
                    data.TryAdd(product,0);
                }

                return;
            }

            var comingSoon = new Product(this, name, url, -1, imgurl, url);
            if (Utils.SatisfiesCriteria(comingSoon, settings))
            {
                data.TryAdd(comingSoon,0);
            }

        }

        private string GetImg(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }

        private string GetPrice(HtmlNode item)
        {
            try
            {
                return item.SelectSingleNode(".//span[@class='price']").InnerHtml;
            }
            
            catch(Exception)
            {
                return null;
            }
        }

        private string GetName(HtmlNode item)
        {
            Debug.WriteLine(item.OuterHtml);
            return item.SelectSingleNode(".//h3").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            Debug.WriteLine(item.OuterHtml);
            return item.SelectSingleNode(".//a").GetAttributeValue("href", null);
        }

        private HtmlNode GetWebpage(HttpClient client, string url, CancellationToken token)
        {
            return client.GetDoc(url, token).DocumentNode;
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            GetWebpage(client, WebsiteBaseUrl, token);

            var resp = GetWebpage(client, productUrl, token);
            if (resp == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to basketrevolution website");
                throw new WebException("Can't connect to website");
            }
            var product = resp.SelectSingleNode("//section[contains(@class, 'product-details')]");
            var name = product.SelectSingleNode("//h1[@class='product-title'][1]").InnerHtml;
            var price = Utils.ParsePrice(product.SelectSingleNode("//span[contains(@id,'product')][1]").InnerHtml);
            var imgurl = product.SelectSingleNode("//figure[1]/img[1]").GetAttributeValue("src", null);

           


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
            string pattern = "\"label\":\"(.*?)\"";
            MatchCollection matches = Regex.Matches(resp.OuterHtml, pattern);

            foreach (Match match in matches)
            {
                var str = match.Groups[1].Value;
                str = Regex.Replace(str, @"\s+", "");
                try
                {
                    var x = decimal.Parse(str);
                    Debug.WriteLine(x.ToString());
                    result.AddSize(x.ToString(), "Unknown");
                }

                catch(Exception)
                {
                    continue;
                }
            }

            return result;
        }
    }
}