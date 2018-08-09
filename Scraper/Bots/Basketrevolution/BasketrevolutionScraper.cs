using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using StoreScraper.Core;
using System.Text.RegularExpressions;
using System;

namespace StoreScraper.Bots.Basketrevolution
{
    public class BasketrevolutionScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Basketrevolution";
        public override string WebsiteBaseUrl { get; set; } = "https://www.basketrevolution.es";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.basketrevolution.es/catalogsearch/result/index/?dir=asc&limit=all&order=created_at&q={0}";
        private const string sizesRegex = "label\":\"([^\\{]*?)\",\"price\":\"0\",\"oldPrice\":\"0\",\"products\":\\[\"(\\d+)\"\\]";

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
            return client.GetDoc(url, token).DocumentNode;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            return document.SelectNodes("//div[@class='item-box']");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "EUR");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-image']/a").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-image']/a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            var priceNode = item.SelectSingleNode(".//span[@class='price']");
            return Convert.ToDouble(Regex.Match(priceNode.InnerText, "(\\d+(,\\d+)?)").Groups[1].Value.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-image']/a/img").GetAttributeValue("src", null);
        }
    }
}
