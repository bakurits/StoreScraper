using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mstanojevic.ShopNordstrom
{

    public class ShopNordstromScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Nordstrom Online";
        public override string WebsiteBaseUrl { get; set; } = "https://shop.nordstrom.com";
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

            /*var sizeCollection = document.SelectNodes("//header[.='Size guide']/../div/table/tbody/tr/td[1][@width='25%']");

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
            string url = WebsiteBaseUrl + "/c/mens-shoes-whats-new?breadcrumb=Home%2FMen%2FNew%20Arrivals%2FShoes";

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//article[contains(@class,'npr-product-module')]");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./h3/a/span/span").InnerHtml.ToLower();
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

            return item.SelectSingleNode("./h3/a/span/span").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./h3/a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            string priceDiv = item.SelectSingleNode("./div/div/span[2]").InnerHtml.Replace("$", "").Replace(",", ".");

            return double.Parse(priceDiv);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/img").GetAttributeValue("src", null);
        }
    }
}
