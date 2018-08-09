using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Higuhigu.Asphaltgold
{
    public class AsphaltgoldScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Asphaltgold";
        public override string WebsiteBaseUrl { get; set; } = "https://asphaltgold.de";
        public override bool Active { get; set; }
        public override Type SearchSettingsType { get; set; } = typeof(AsphaltgoldSearchSettings);

        private static readonly string[] Links = { "https://asphaltgold.de/en/sneaker/", "https://asphaltgold.de/en/apparel/" };

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

            HtmlNodeCollection sizesNodeCollection = document.SelectNodes("//li[contains(@id, 'option_')]");

            foreach(var sizeNode in sizesNodeCollection)
            {
                    details.AddSize(sizeNode.SelectSingleNode("./div").InnerText, "Unknown");
                
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
            AsphaltgoldSearchSettings.ItemTypeEnum typeEnum = ((AsphaltgoldSearchSettings)settings).ItemType;
            string url = Links[(int)typeEnum];
            var document = GetWebpage(url, token);
            return document.SelectNodes("//section[@class='item']");
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

        private double GetPrice(HtmlNode item)
        {
            return Convert.ToDouble(item.SelectSingleNode("./a").GetAttributeValue("data-price", null));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null);
        }
    }
}
