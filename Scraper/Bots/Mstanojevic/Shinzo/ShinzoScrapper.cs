using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StoreScraper.Bots.Mstanojevic.Shinzo
{
    public class ShinzoScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Shinzo";
        public override string WebsiteBaseUrl { get; set; } = "http://www.shinzo.paris";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
            Console.WriteLine(itemCollection.Count);
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, item);
#endif
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


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            ProductDetails details = new ProductDetails();


            var strDoc = document.InnerHtml;

            if (strDoc.Contains("var combinations = "))
            {

                var start = strDoc.IndexOf("var combinations = ");


                var trimmed = strDoc.Substring(start, strDoc.Length - start);
                var end = trimmed.IndexOf(";");

                trimmed = trimmed.Substring(0, end);

                trimmed = trimmed.Replace("var combinations = ", "");

                JObject obj = JObject.Parse(trimmed);
                foreach (var attr in obj)
                {
                    
                        if ( int.Parse(attr.Value["quantity"].ToString()) > 0 )
                        {
                            details.AddSize(attr.Value["attributes_values"].First.First.ToString(), attr.Value["quantity"].ToString());

                        }
                    

                        
                }
            }

                    /*var sizeCollection = document.SelectNodes("//div[@class='attribute_list']/ul/li/label");

                    foreach (var size in sizeCollection)
                    {
                        string sz = size.InnerHtml;
                        if (sz.Length > 0)
                        {
                            details.AddSize(sz, "Unknown");
                        }

                    }*/

                    return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private HtmlNode GetPostWebPage(string url, FormUrlEncodedContent fields, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);

            var document = client.PostDoc(url, token, fields).DocumentNode;
            return document;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
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

            var document = GetPostWebPage(url, postParams, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//div[@class='product-inner']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./div[@class='product-info']/h3/a").InnerText.ToLower();
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

            /*if (!(price >= settings.MinPrice && price <= settings.MaxPrice) && (settings.MaxPrice != 0 && settings.MinPrice != 0))
            {
                return;
            }*/


            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "EUR");
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

            return item.SelectSingleNode("./div[@class='product-info']/h3/a").InnerText;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-info']/h3/a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            var node = item.SelectSingleNode("./div[@class='product-info']/div[@class='content_price']/a/span");
            if (node != null)
            {
                string priceDiv = item.SelectSingleNode("./div[@class='product-info']/div[@class='content_price']/a/span").InnerHtml.Replace("€", "").Replace("&euro;", "").Replace("$", "").Replace(",",".");

                return double.Parse(priceDiv);
            }
            else
            {
                return 0;
            }
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-thumb']/div/a/img").GetAttributeValue("src", null);
        }
    }
}
