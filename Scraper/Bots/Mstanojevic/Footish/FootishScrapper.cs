using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mstanojevic.Footish
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
                }catch
                {

                }
                var product = new Product(this, item["Name"].ToString(), item["ProductUrl"].ToString(), price, item["Images"][0]["Url"].ToString(), item["Id"].ToString(), "EUR");
                if (Utils.SatisfiesCriteria(product, settings))
                {
                    listOfProducts.Add(product);
                }

            }
        }


        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
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


        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            //var document = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();

            /*var sizeCollection = document.SelectNodes("//select[@onchange='Products_UpdateAttributeValue(this);']/option");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerHtml;
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
                }

            }*/


            string restApiUrl = "https://www.footish.se/Services/Rest/v2/json/en-GB/EUR/products/"+product.Id;
            //Console.WriteLine(restApiUrl);
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var response = Utils.GetParsedJson(client, restApiUrl, token);
            //Console.WriteLine(response["TotalProducts"]);

            foreach (var item in response["ProductItems"])
            {
                foreach(var attr in item["Attributes"])
                {
                    if (int.Parse(attr["StockLevel"].ToString()) > 0)
                    {
                        details.AddSize(attr["Value"].ToString(), attr["StockLevel"].ToString());
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
            var invalidKeywords = settings.NegKeyWrods.ToLower().Split(' ');
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
            double price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "USD");
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

        private double GetPrice(HtmlNode item)
        {
            try
            {
                string priceDiv = item.SelectSingleNode("./div[3]/div[3]/div/span").InnerHtml.Replace("&nbsp;", "").Replace("kr", "").Replace("$", "").Replace("€", "").Replace(",", ".");

                return double.Parse(priceDiv);
            } catch
            {
                return 0;
            }
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a/img").GetAttributeValue("src", null);
        }
    }
}
