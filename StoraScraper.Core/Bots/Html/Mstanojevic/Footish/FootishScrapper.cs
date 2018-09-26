using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Mstanojevic.Footish
{

    public class FootishScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Footish.se";
        public override string WebsiteBaseUrl { get; set; } = "https://www.footish.se";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();

         
                string restApiUrl = "https://www.footish.se/Services/Rest/v2/json/en-GB/EUR/search/full/" + settings.KeyWords + "/200/1";
                var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
                var response = Utils.GetParsedJson(client, restApiUrl, token);
                Console.WriteLine(response["TotalProducts"]);

                foreach (var item in response["ProductItems"])
                {
                    double price = 0;
                    try
                    {
                        string str = item["DiscountedPrice"].ToString();
                        if (str == "-1")
                        {
                            str = item["Price"].ToString();
                        }
                        str = str.Substring(str.Length - 2);
                        price = double.Parse(str);
                    }
                    catch
                    {

                    }
                    var product = new Product(this, item["Name"].ToString(), item["ProductUrl"].ToString(), price, item["Images"][0]["Url"].ToString(), item["Id"].ToString(), "EUR");
                    if (Utils.SatisfiesCriteria(product, settings))
                    {
                        listOfProducts.Add(product);
                    }

                }

            
           
        }


        private readonly List<String> newArrivalPageUrls = new List<string>
        {
            "https://www.footish.se/en/sneakers",
            "https://www.footish.se/en/klader",
            "https://www.footish.se/en/kepsar-mossor",
            "https://www.footish.se/en/ovrigt",
        };



        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            Task.WhenAll(newArrivalPageUrls.Select(url => GetProductsForPage(url, data, null, token))).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }


        private async Task GetProductsForPage(string url, ConcurrentDictionary<Product, byte> data,
            SearchSettingsBase settings, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var page = (await client.GetDocTask(url, token)).DocumentNode;
            HtmlNodeCollection collection = page.SelectNodes("//article[@class='product-wrapper']");

            foreach (var item in collection)
            {
                token.ThrowIfCancellationRequested();
                Product product = GetProduct(item);
                if (product != null && (settings == null || Utils.SatisfiesCriteria(product, settings)))
                {
                    data.TryAdd(product, 0);
                }
            }

        }


        private Product GetProduct(HtmlNode item)
        {
            try
            {
                string name = GetName(item).TrimEnd();
                string url = GetUrl(item);
                var price = GetPrice(item);
                string imageUrl = GetImageUrl(item);
                return new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            }
            catch
            {
                return null;
            }
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            var price = Utils.ParsePrice(document.SelectSingleNode("//meta[@property='og:price:amount']").GetAttributeValue("content","") + " " + document.SelectSingleNode("//meta[@property='og:price:currency']").GetAttributeValue("content", ""));
            
            
            string name = document.SelectSingleNode("//span[@itemprop='name']").InnerText;
            string image = WebsiteBaseUrl + document.SelectSingleNode("//div[@class='product-images']/div/div/img").GetAttributeValue("src", "");

            string brand = null;
            if (document.SelectSingleNode("//div[@class='product-title']/a") != null)
            {
                brand = document.SelectSingleNode("//div[@class='product-title']/a").InnerText;
            }

            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this,
                BrandName = brand
            };
            var strDoc = document.InnerHtml;

            if (strDoc.Contains("var JetshopData="))
            {
                var start = strDoc.IndexOf("var JetshopData=");
                var trimmed = strDoc.Substring(start, strDoc.Length - start);
                var end = trimmed.IndexOf(";");

                trimmed = trimmed.Substring(0, end);

                trimmed = trimmed.Replace("var JetshopData=", "");
                JObject obj = JObject.Parse(trimmed);

                string productId = obj["ProductId"].ToString();
                string restApiUrl = "https://www.footish.se/Services/Rest/v2/json/en-GB/EUR/products/"+productId;
                //Console.WriteLine(restApiUrl);
                var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
                var response = Utils.GetParsedJson(client, restApiUrl, token);
                //Console.WriteLine(response["TotalProducts"]);

                foreach (var item in response["ProductItems"])
                {
                    foreach (var attr in item["Attributes"])
                    {
                        if (int.Parse(attr["StockLevel"].ToString()) > 0)
                        {
                            details.AddSize(attr["Value"].ToString(), attr["StockLevel"].ToString());
                        }

                    }
                }



            }

            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

       
   
        
       
    

        private string GetName(HtmlNode item)
        {
           
            return item.SelectSingleNode("./div[@class='product-info']/div/h3/a").GetAttributeValue("title", "");
        }

        private string GetUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode("./div[@class='product-info']/div/h3/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*try
            {
                string priceDiv = item.SelectSingleNode("./div[3]/div[3]/div/span").InnerHtml.Replace("&nbsp;", "").Replace("kr", "").Replace("$", "").Replace("€", "").Replace(",", ".");

                return double.Parse(priceDiv);
            } catch
            {
                return 0;
            }*/

            return Utils.ParsePrice(item.SelectSingleNode("./div[3]/div[3]/div/span").InnerHtml.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a/img").GetAttributeValue("src", null);
        }
    }
}
