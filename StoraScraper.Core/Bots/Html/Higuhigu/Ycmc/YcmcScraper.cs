using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace StoreScraper.Bots.Html.Higuhigu.Ycmc
{
    public class YcmcScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Ycmc";
        public override string WebsiteBaseUrl { get; set; } = "https://www.ycmc.com/";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.ycmc.com/new-arrivals.html?limit=120";
 

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
            string url = SearchFormat;
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to ycmc website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//li[@class='item']");
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
            string name = GetName(item).Trim();
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

        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./*[@class='product-name']/a").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./h2/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            string priceStr = item.SelectSingleNode(".//span[@class='price'][last()]").InnerText;
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div[@class='product-image-wrapper']/a/img").GetAttributeValue("src", null);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to ycmc website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var sizeNodes = root.SelectNodes("//span[contains(@class, 'size_group_code_item')]/a");
            var sizes = sizeNodes?.Select(node => node?.InnerText).ToList();

            var name = root.SelectSingleNode("//div[@class='product-name']/h1")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode(".//div[@class='price-box']//span[@class='price'][last()]");
            var price = Utils.ParsePrice(priceNode?.InnerText.Replace(",", "."));
            var image = root.SelectSingleNode("//ul[@class='product-image-thumbs']/li/img")?.GetAttributeValue("src", null);

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
