using System.Collections.Generic;
using System.Threading;
using System.Linq;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Core;
using StoreScraper.Models;
using System.Text.RegularExpressions;
using System;

namespace StoreScraper.Bots.Consortium
{
    public class ConsortiumScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Consortium";
        public override string WebsiteBaseUrl { get; set; } = "http://www.consortium.co.uk";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.consortium.co.uk/latest";
        private const string priceRegex = "£(\\d+.\\d+)";
        private const string sizesRegex = "\"label\":\"(.*?)\",\"products\":\\[(\\d+)\\]";

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

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var document = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();

            Match match = Regex.Match(document.InnerHtml, sizesRegex);

            while (match.Success)
            {
                var sz = match.Groups[1].Value;
                if (!details.SizesList.Exists(szInfo => szInfo.Key == sz) && sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
                }
                match = match.NextMatch();
            }
            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient();
            var document = client.GetDoc(url, token).DocumentNode;
            return client.GetDoc(url, token).DocumentNode;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = SearchFormat;
            var document = GetWebpage(url, token);
            return document.SelectNodes("//li[@class='item text-center']");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "GBP");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                var keyWordSplit = settings.KeyWords.Split(' ');
                if (keyWordSplit.All(keyWord => product.Name.ToLower().Contains(keyWord.ToLower())))
                    listOfProducts.Add(product);
            }
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./img").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./img").GetAttributeValue("onclick", null).Split('=')[1].Replace("'", "");
        }

        private double GetPrice(HtmlNode item)
        {
            Match match = Regex.Match(item.InnerHtml, priceRegex);
            double price = -1;
            while (match.Success)
            {
                price = Convert.ToDouble(match.Groups[1].Value);
                match = match.NextMatch();
            }
            return price;
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./img").GetAttributeValue("src", null);
        }
    }
}
