using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Higuhigu.Uptherestore
{
    public class UptherestoreScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Uptherestore";
        public override string WebsiteBaseUrl { get; set; } = "https://uptherestore.com";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://uptherestore.com/latest/";

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
            //li
            var document = GetWebpage(productUrl, token);
            ProductDetails details = new ProductDetails();

            var sizeCollection = document.SelectNodes("//li[contains(@id, 'option')]/a/span");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerText.Trim();
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
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
            string url = SearchFormat;
            var document = GetWebpage(url, token);
            return document.SelectNodes("//li[contains(@class, 'item')]/div");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "USD");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                var keyWordSplit = settings.KeyWords.Split(' ');
                if (keyWordSplit.All(keyWord => product.Name.ToLower().Contains(keyWord.ToLower())))
                    listOfProducts.Add(product);
            }
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode(".//a").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            var priceNode = item.SelectSingleNode(".//span[@class='regular-price']/span");
            if (priceNode == null)
            {
                priceNode = item.SelectSingleNode(".//p[@class='special-price']/span");
            }
            if (priceNode == null) { return -1; }
            return Convert.ToDouble(Regex.Match(priceNode.InnerText, @"(\d+(\.\d+)?)").Groups[0].Value);

        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }
    }
}
