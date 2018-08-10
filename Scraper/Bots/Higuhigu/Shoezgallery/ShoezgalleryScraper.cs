using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Higuhigu.Shoezgallery
{
    public class ShoezgalleryScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Shoezgallery";
        public override string WebsiteBaseUrl { get; set; } = "https://www.shoezgallery.com";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.shoezgallery.com/en/recherche?orderby=position&orderway=desc&r=true&search_query=sneaker&submit_search={0}";
        private const string priceRegex = "(\\d+(,\\d+)?) €";

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

            var sizeCollection = document.SelectNodes("//a[contains(@class, 'attribute_link')]");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerText;
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
            return document.SelectNodes("//div[contains(@id, 'category_product_')]");
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
            return item.SelectSingleNode(".//img").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-image']/a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            Match match = Regex.Match(item.InnerHtml, priceRegex);
            double price = -1;
            while (match.Success)
            {
                price = Convert.ToDouble(match.Groups[1].Value.Replace(",", "."));
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
