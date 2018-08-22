using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using System.Net;
using System.Linq;

namespace StoreScraper.Bots.Higuhigu._43Einhalb
{
    public class EinhalbScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "43Einhalb";
        public override string WebsiteBaseUrl { get; set; } = "http://www.43einhalb.com";

        public override bool Active { get; set; }

        private const string SearchFormat = @"http://www.43einhalb.com/en/search/{0}/page/1/sort/date_new/perpage/72";

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
                Logger.Instance.WriteErrorLog($"Can't Connect to einhalb website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//li[@class='item']");
            if(items == null)
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
            string name = GetName(item).TrimEnd();
            string url = WebsiteBaseUrl + GetUrl(item);
            var price = GetPrice(item);
            string imageUrl = WebsiteBaseUrl + GetImageUrl(item);
            var product = new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            if (!Utils.SatisfiesCriteria(product, settings)) return;
            listOfProducts.Add(product);
        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode(".//img[@class='current']").GetAttributeValue("alt", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//a[1]").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            var priceSpan = item.SelectSingleNode(".//span[@class='pPrice']");
            var priceSpanSecond = priceSpan.SelectSingleNode(".//span[@class='newPrice']");
            if (priceSpanSecond != null) priceSpan = priceSpanSecond;
            string priceStr = priceSpan.InnerText.Trim();
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//img[@class='current']").GetAttributeValue("src", null);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to einhalb website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var sizeNodes = root.SelectNodes("//select[@class='customSelectBox']/option[@class='']");
            var sizes = sizeNodes?.Select(node => node.InnerText.Trim()).ToList();

            var name = root.SelectSingleNode("//span[@class='productName']")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@itemprop='price']");
            var price = Utils.ParsePrice(priceNode?.InnerText);
            var image = root.SelectSingleNode("//a[@class='galleriaTrigger']/img")?.GetAttributeValue("src", null);

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

            foreach (var size in sizes)
            {
                result.AddSize(size, "Unknown");
            }

            return result;
        }
    }
}
