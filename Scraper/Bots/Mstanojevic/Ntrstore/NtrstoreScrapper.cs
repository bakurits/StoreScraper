using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StoreScraper.Bots.Mstanojevic.Ntrstore
{

    public class NtrstoreScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "NTR store";
        public override string WebsiteBaseUrl { get; set; } = "http://www.ntrstore.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
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

            if (strDoc.Contains("var spConfig = new Product.Config({"))
            {

                var start = strDoc.IndexOf("var spConfig = new Product.Config({");


                var trimmed = strDoc.Substring(start, strDoc.Length - start);
                var end = trimmed.IndexOf(");");

                trimmed = trimmed.Substring(0, end);

                trimmed = trimmed.Replace("var spConfig = new Product.Config(", "");
                JObject obj = JObject.Parse(trimmed);

                foreach (var attr in obj["attributes"])
                {

                    foreach (var x in attr)
                    {
                        if (x["code"].ToString() == "us_size_mens")
                        {
                            foreach (var option in x["options"])
                            {
                                details.AddSize(option["label"].ToString(), "Unknown");
                            }
                        }


                    }
                }
            }

            /*var sizeCollection = document.SelectNodes("//select[contains(@id,'attribute')/option]");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerHtml;
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
                }

            }
            */

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
            string url = WebsiteBaseUrl + "/catalogsearch/result/index/?dir=asc&limit=36&order=entity_id&q=" + settings.KeyWords;

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//div[@class='item-area']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./div/h2[@class='product-name']").InnerHtml.ToLower();
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
            if (!CheckForValidProduct(item, settings)) return;
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);

            if (!(price >= settings.MinPrice && price <= settings.MaxPrice))
            {
                return;
            }


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

            return item.SelectSingleNode("./div/h2[@class='product-name']/a").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/h2[@class='product-name']/a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            string priceDiv = item.SelectSingleNode("./div/div/span[@class='regular-price']/span").InnerHtml.Replace("€", "");

            return double.Parse(priceDiv);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a/img").GetAttributeValue("src", null);
        }
    }
}
