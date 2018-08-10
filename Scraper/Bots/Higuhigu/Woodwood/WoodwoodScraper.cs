using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Higuhigu.Woodwood
{
    public class WoodwoodScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Woodwood";
        public override string WebsiteBaseUrl { get; set; } = "https://www.woodwood.com/";
        public override bool Active { get; set; }
        public override Type SearchSettingsType { get; set; } = typeof(WoodwoodSearchSettings);
        private static readonly string[] Links = { "https://www.woodwood.com/men/new-arrivals", "https://www.woodwood.com/women/new-arrivals" };

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

            HtmlNodeCollection sizesNodeCollection = document.SelectSingleNode("//select[contains(@id, 'form-size')]").SelectNodes(".//option[not(@value='')]");

            foreach (var sizeNode in sizesNodeCollection)
            {
                details.AddSize(sizeNode.InnerText, "Unknown");

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
            WoodwoodSearchSettings.GenderEnum Gender = ((WoodwoodSearchSettings)settings).Gender;
            string url = Links[(int)Gender];
            var document = GetWebpage(url, token);
            return document.SelectSingleNode("//ul[@id='landingpage-lister-list']").SelectNodes("./li");
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
            return item.SelectSingleNode(".//img").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            return Convert.ToDouble(item.SelectSingleNode(".//span[@class='list-commodity-price']").InnerText.Split(' ')[1]);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }
    }
}
