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

namespace StoreScraper.Bots.Higuhigu.Asphaltgold
{
    public class AsphaltgoldScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Asphaltgold";
        public override string WebsiteBaseUrl { get; set; } = "http://asphaltgold.de";
        public override bool Active { get; set; }
        public override Type SearchSettingsType { get; set; } = typeof(AsphaltgoldSearchSettings);

        private static readonly string[] Links = { "http://asphaltgold.de/en/sneaker/", "http://asphaltgold.de/en/apparel/" };

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            AsphaltgoldSearchSettings.ItemTypeEnum itemEnum;
            try
            {
                itemEnum = ((AsphaltgoldSearchSettings)settings).ItemType;
            }
            catch
            {
                itemEnum = AsphaltgoldSearchSettings.ItemTypeEnum.Both;
            }
            listOfProducts = new List<Product>();
            switch (itemEnum)
            {
                case AsphaltgoldSearchSettings.ItemTypeEnum.Sneakers:
                    FindItemsForType(listOfProducts, settings, token, 0);
                    break;
                case AsphaltgoldSearchSettings.ItemTypeEnum.Apparel:
                    FindItemsForType(listOfProducts, settings, token, 1);
                    break;
                default:
                    FindItemsForType(listOfProducts, settings, token, 0);
                    FindItemsForType(listOfProducts, settings, token, 1);
                    break;
            }
        }

        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token);
            return document;
        }

        private void FindItemsForType(List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, int type)
        {
            string url = Links[(int)type];
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to asphaltgold website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//section[@class='item']");
            if (items == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected Html!!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexpected Html");
            }
            foreach (var item in items)
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

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
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
            string priceStr = item.SelectSingleNode(".//span[@itemprop='price'][last()]").InnerText;
            return Utils.ParsePrice(priceStr);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to asphaltgold website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var sizeNodes = root.SelectNodes("//li[contains(@id, 'option_')]/div");
            var sizes = sizeNodes?.Select(node => node.InnerText.Trim()).ToList();

            var name = root.SelectSingleNode("//span[@class='attr-name']").InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@itemprop='price'][last()]");
            var price = Utils.ParsePrice(priceNode.InnerText);
            var image = root.SelectSingleNode("//meta[@property='og:image']").GetAttributeValue("content", null);

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
