using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using System.Net;


namespace StoreScraper.Bots.Higuhigu.Nighshop
{
    public class NighshopScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Nighshop";
        public override string WebsiteBaseUrl { get; set; } = "https://www.nighshop.com/";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.nighshop.com/";

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

        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token);
            return document;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to nighshop website");
                throw new WebException("Can't connect to website");
            }
            if (document == null)
            {

            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//li[contains(@class, 'item col')]/div");
            if (items == null)
            {
                Logger.Instance.WriteErrorLog("Uncexpected Html!!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Undexpected Html");
            }
            return items;
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

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (GetStatus(item)) return;
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            var price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                var keyWordSplit = settings.KeyWords.Split(' ');
                if (keyWordSplit.All(keyWord => product.Name.ToLower().Contains(keyWord.ToLower())))
                    listOfProducts.Add(product);
            }
        }

        private bool GetStatus(HtmlNode item)
        {
            return item.InnerHtml.Contains("soon") || item.InnerHtml.Contains("sold-out");
        }

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            var priceSpan = item.SelectSingleNode(".//span[@class='price'][last()]");
            string priceStr = priceSpan.InnerText;
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {

            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to nighshop website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var name = root.SelectSingleNode("//div[@class='product-name']/h1")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@class='price'][last()]");
            var price = Utils.ParsePrice(priceNode?.InnerText);
            var image = root.SelectSingleNode("//img[@class='owl-lazy']")?.GetAttributeValue("data-src", null);

            ProductDetails result = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };

            var sizeCollection = root.SelectNodes("//div[contains(@class, 'attribute-item')]");
            foreach (var size in sizeCollection)
            {
                string sz = size.InnerText.Trim();
                if (sz.Length > 0)
                {
                    if (!(size.GetAttributeValue("class", null).Contains("disabled")))
                        result.AddSize(sz, "Unknown");
                }

            }
            return result;
        }
    }
}
