using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
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

        private readonly List<string> _newArrivalPageUrls = new List<string>
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

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            Task.WhenAll(_newArrivalPageUrls.Select(url => Scrap(client, url, data, null, token))).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }

        private async Task Scrap(HttpClient client, string url, ConcurrentDictionary<Product, byte> data, SearchSettingsBase settings, CancellationToken token)
        {
            await client.GetDocTask(WebsiteBaseUrl, token);
            var rooturl = url;
            var document =(await client.GetDocTask(rooturl,token));
            var rootSearch = document.DocumentNode;

            var initialProducts = rootSearch.SelectNodes("//div[@class='product']");

            if (initialProducts == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected Html");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexpected Html");
            }

            foreach (var item in initialProducts)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(data, item, settings);
#else
                LoadSingleProductTryCatchWrapper(data, item, settings);
#endif
            }

            
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var data = new ConcurrentDictionary<Product,byte>();
            var rooturl = string.Format(SearchUrl, settings.KeyWords);
            Scrap(client, rooturl,data,settings,token).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }
        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="data"></param>
        /// <param name="child"></param>
        /// <param name="settings"></param>
        private void LoadSingleProductTryCatchWrapper(ConcurrentDictionary<Product, byte> data, HtmlNode child, SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(data, child, settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private void LoadSingleProduct(ConcurrentDictionary<Product,byte> data,HtmlNode item, SearchSettingsBase settings)
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
            return item.SelectSingleNode(".//h3").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
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
                Logger.Instance.WriteErrorLog($"Can't Connect to ruvilla website");
                throw new WebException("Can't connect to website");
            }
            var product = resp.SelectSingleNode("//section[contains(@class, 'product-details')]");
            var name = product.SelectSingleNode("//h1[@class='product-title'][1]").InnerText;
            var priceNode = product.SelectSingleNode("//div[@class='price-box']");
            var price = Utils.ParsePrice(priceNode.SelectSingleNode("//span[contains(@id,'product')][1]").InnerText);
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
            string pattern = @"var spConfig = new Product.Config\((.*?)\)";
            var match = Regex.Match(resp.OuterHtml, pattern).Groups[1].Value;
            var jObj = JObject.Parse(match);
            var sizes = jObj.SelectToken("attributes").SelectToken("196").SelectToken("options").Children();
            foreach (var size in sizes)
            {
                result.AddSize(size.Value<string>("label"),"Unknown");
            }
            return result;
        }
    }
}