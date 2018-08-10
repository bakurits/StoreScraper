// TO DO: ADD RELEASE DATE FOR COMING SOON PRODUCTS

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

namespace StoreScraper.Bots.Higuhigu.Overkill
{
    public class OverkillScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Overkill";
        public override string WebsiteBaseUrl { get; set; } = "https://www.overkillshop.com/";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.overkillshop.com/en/new-products.html?limit=150";
        private const string priceRegex = "\\€(\\d+(\\.\\d+)?)";
        private const string sizesRegex = "label\":\"([^\\{]*?)\",\"price\":\"0\",\"oldPrice\":\"0\",\"products\":\\[\"(\\d+)\"\\]";
        // "label":"36","price":"0","oldPrice":"0","products"


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

            Match match = Regex.Match(document.InnerHtml, sizesRegex);

            while (match.Success)
            {
                var sz = match.Groups[1].Value;
                if (!details.SizesList.Exists(size => size.Key == sz) && sz.Length > 0 && !(sz.Contains("{")))
                {
                    details.AddSize(sz, "Unknown");
                }
                match = match.NextMatch();
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
            return document.SelectSingleNode("//div[@class='category-products']").SelectNodes(".//li[contains(@class, 'item')]");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (GetStatus(item)) return;
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "EUR");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                var keyWordSplit = settings.KeyWords.Split(' ');
                if (keyWordSplit.All(keyWord => product.Name.ToLower().Contains(keyWord.ToLower())))
                    listOfProducts.Add(product);
            }
        }


        private bool GetStatus(HtmlNode item)
        {
            return item.InnerHtml.Contains("Coming Soon");
        }


        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a").GetAttributeValue("href", null);
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
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }
    }
}
