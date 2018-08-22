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

namespace StoreScraper.Bots.Higuhigu.Woodwood
{
    public class WoodwoodScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Woodwood";
        public override string WebsiteBaseUrl { get; set; } = "http://www.woodwood.com/";
        public override bool Active { get; set; }
        public override Type SearchSettingsType { get; set; } = typeof(WoodwoodSearchSettings);
        private static readonly string[] Links = { "http://www.woodwood.com/men/new-arrivals", "http://www.woodwood.com/women/new-arrivals" };

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            WoodwoodSearchSettings.GenderEnum genderEnum;
            try
            {
                genderEnum = ((WoodwoodSearchSettings)settings).Gender;
            }
            catch
            {
                genderEnum = WoodwoodSearchSettings.GenderEnum.Both;
            }
            listOfProducts = new List<Product>();
            switch (genderEnum)
            {
                case WoodwoodSearchSettings.GenderEnum.Man:
                    FindItemsForGender(listOfProducts, settings, token, 0);
                    break;
                case WoodwoodSearchSettings.GenderEnum.Woman:
                    FindItemsForGender(listOfProducts, settings, token, 1);
                    break;
                default:
                    FindItemsForGender(listOfProducts, settings, token, 0);
                    FindItemsForGender(listOfProducts, settings, token, 1);
                    break;
            }
        }

        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token);
            return document;
        }

        private void FindItemsForGender(List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, int Gender)
        {
            string url = Links[Gender];
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to woodwood website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//ul[@id='landingpage-lister-list']/li");
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
            if (item.InnerHtml.Contains("Coming soon")) return;
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
            return item.SelectSingleNode(".//img").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            string priceStr = item.SelectSingleNode(".//span[@class='list-commodity-price']").InnerText;
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
                Logger.Instance.WriteErrorLog($"Can't Connect to woodwood website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var sizeNodes = root.SelectNodes("//select[contains(@id, 'form-size')]//option[not(@value='')]");
            var sizes = sizeNodes?.Select(node => node.InnerText.Trim()).ToList();

            var name = root.SelectSingleNode("//h1[@class='headline']")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@class='price']");
            var price = Utils.ParsePrice(priceNode?.InnerText);
            var image = root.SelectSingleNode("//a[@id='commodity-show-image']/img")?.GetAttributeValue("src", null);

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
