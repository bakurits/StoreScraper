using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Higuhigu._43Einhalb
{
    public class EinhalbScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "43Einhalb";
        public override string WebsiteBaseUrl { get; set; } = "https://www.43einhalb.com";
        public override bool Active { get; set; }
        private const string SearchFormat = @"https://www.43einhalb.com/en/search/{0}/page/1/sort/date_new/perpage/72";
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


        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var document = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();

            var sizeCollection = document.SelectNodes("//select[@class='customSelectBox']/option[@class='']");

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
            string url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;
            return document.SelectNodes("//li[@class='item']");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = WebsiteBaseUrl + GetUrl(item);
            double price = GetPrice(item);
            string imageUrl = WebsiteBaseUrl + GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "EUR");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode(".//img[@class='current']").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//a[1]").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            var priceSpan = item.SelectSingleNode(".//span[@class='pPrice']");
            var priceSpanSecond = priceSpan.SelectSingleNode(".//span[@class='newPrice']");
            if (priceSpanSecond != null) priceSpan = priceSpanSecond;
            string priceDiv = priceSpan.InnerText.Trim();

            return Convert.ToDouble(Regex.Match(priceDiv, "(\\d+(\\.\\d+)?)").Groups[1].Value);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//img[@class='current']").GetAttributeValue("src", null);
        }
    }
}
