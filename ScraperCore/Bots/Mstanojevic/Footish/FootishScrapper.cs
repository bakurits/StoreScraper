using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace StoreScraper.Bots.Mstanojevic.Footish
{

    public class FootishScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Footish.se";
        public override string WebsiteBaseUrl { get; set; } = "http://www.footish.se";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();

            if (settings.Mode == ScraperCore.Models.SearchMode.SearchAPI)
            {
                string restApiUrl = "http://www.footish.se/Services/Rest/v2/json/en-GB/EUR/search/full/" + settings.KeyWords + "/200/1";
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
            else
            {

                var values = new Dictionary<string, string>
            {
                {"controller", "search" },
                {"orderby", "position"},
                {"orderway", "desc"},
                {"search_query", settings.KeyWords},

            };

                var postParams = new FormUrlEncodedContent(values);



                string url = WebsiteBaseUrl + "/en/search";
                //todo higuhigu there is compilation error
                
                //var document = GetPostWebPage(url, postParams, token);


            }
        }


        private void LoadSingleProductTryCatchWrapper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, item);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            var price = Utils.ParsePrice(document.SelectSingleNode("//meta[@property='og:price:amount']").GetAttributeValue("content","") + " " + document.SelectSingleNode("//meta[@property='og:price:currency']").GetAttributeValue("content", ""));
            
            
            string name = document.SelectSingleNode("//span[@itemprop='name']").InnerText;
            string image = WebsiteBaseUrl + document.SelectSingleNode("//div[@class='product-images']/div/div/img").GetAttributeValue("src", "");

            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
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
                string restApiUrl = "http://www.footish.se/Services/Rest/v2/json/en-GB/EUR/products/"+productId;
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

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            //string url = string.Format(SearchFormat, settings.KeyWords);
            string url = WebsiteBaseUrl + "/en/sneakers";

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//article[@class='product-wrapper']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./div[3]/div/h3/a").GetAttributeValue("title", "").ToLower();
            var validKeywords = settings.KeyWords.ToLower().Split(' ');
            var invalidKeywords = settings.NegKeyWords.ToLower().Split(' ');
            foreach (var keyword in validKeywords)
            {
                if (!title.Contains(keyword))
                    return false;
            }


            foreach (var keyword in invalidKeywords)
            {
                if (keyword == "")
                    continue;
                if (title.Contains(keyword))
                    return false;
            }


            return true;

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            //if (!CheckForValidProduct(item, settings)) return;
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            var price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private bool GetStatus(HtmlNode item)
        {
            return true;
        }

        private string GetName(HtmlNode item)
        {
            //Console.WriteLine("GetName");
            //Console.WriteLine(item.SelectSingleNode("./a").GetAttributeValue("title", ""));

            return item.SelectSingleNode("./div[3]/div/h3/a").GetAttributeValue("title", "");
        }

        private string GetUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode("./div[3]/div/h3/a").GetAttributeValue("href", null);
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
