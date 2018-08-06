using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using StoreScraper.Core;
using System.Text.RegularExpressions;
using System;

namespace StoreScraper.Bots.UrbanjunglestoreScraper
{
    public class UrbanjunglestoreScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Urbanjunglestore";
        public override string WebsiteBaseUrl { get; set; } = "https://www.urbanjunglestore.com";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.urbanjunglestore.com/it/catalogsearch/result/?cat=137&q={0}";
        private const string imageRegex = "src=\"https://www.urbanjunglestore.com/media/catalog/(.*?)\"";
        private const string imageRegexBase = "https://www.urbanjunglestore.com/media/catalog/";
        private const string sizesRegex = "title=\"USA(\\d+(,\\d+)?)-EU(\\d+(,\\d+)?)\"";

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

            Match match = Regex.Match(document.InnerHtml.Replace(" ", ""), sizesRegex);

            while (match.Success)
            {
                var sz = match.Groups[2].Value.ToString();
                if (!details.SizesList.Contains(sz) && sz.Length > 0)
                {
                    details.SizesList.Add(sz);
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
            return document.SelectNodes("//li[@class='item urban-widget']");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (GetStatus(item)) return;
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);
            if (price < 0) return;
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "EUR");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        
        
        private bool GetStatus(HtmlNode item)
        {
            return !(item.SelectNodes(".//div[@class='sold-out']") == null);
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./h2[@class='product-name']/a").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./h2[@class='product-name']/a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            var priceNode = item.SelectSingleNode(".//span[@class='regular-price']/span");
            if(priceNode==null)
            {
                priceNode = item.SelectSingleNode(".//p[@class='special-price']/span");
            }
            if (priceNode == null) { return -1; }
            return Convert.ToDouble(Regex.Match(priceNode.InnerText, @"(\d+.\d+)").Groups[0].Value.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return imageRegexBase + Regex.Match(item.InnerHtml, imageRegex).Groups[1].Value;
        }
    }
}
