using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace StoreScraper.Bots.Bakurits.Rimowa
{
    public class RimowaScraper : ScraperBase
    {
        private const string SearchFormat =
            @"http://www.rimowa.com/search?q={0}&srule=newest&sz=12&start={1}&format=page-element";

        public override string WebsiteName { get; set; } = "Rimowa";
        public override string WebsiteBaseUrl { get; set; } = "http://www.rimowa.com/";
        private int MaxItemCount { get; set; } = 48;
        public override bool Active { get; set; }

        public override Type SearchSettingsType { get; set; } = typeof(SearchSettingsBase);


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            GetProducts(listOfProducts, settings, token, SearchFormat);
        }

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            GetProducts(listOfProducts, null, token,
                "https://www.rimowa.com/luggage/all-luggage/?srule=newest&sz=12&start={0}");
        }

        private void GetProducts(List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token,
            string urlFormat)
        {
            var urls = new List<string>();
            var seenProducts = 0;
            for (var i = 0; seenProducts < MaxItemCount; i++)
            {
                var url = settings != null
                    ? string.Format(urlFormat, settings.KeyWords, i * 12)
                    : string.Format(urlFormat, i * 12);
                urls.Add(url);
                seenProducts += 12;
            }

            var pages = GetPageTask(urls, token).Result;
            foreach (var page in pages)
            {
                var items = page.SelectNodes("//li[contains(@class, 'grid-tile')]");
                if (items == null) break;
                listOfProducts.AddRange(items.Select(GetProduct).Where(product =>
                    product != null && (settings == null || Utils.SatisfiesCriteria(product, settings))));
            }
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient();
            var doc = client.GetDoc(productUrl, token);
            var page = doc.DocumentNode;
            var infoContainer = page.SelectSingleNode("//div[@id = 'pchange-target']/div");

            var collectionName =
                infoContainer.SelectSingleNode("./span[@itemprop = 'name']").InnerHtml.EscapeNewLines();
            var productName = infoContainer.SelectSingleNode("./h1[@itemprop = 'name']").InnerHtml.EscapeNewLines();
            var name = $"{collectionName} - {productName}";
            var priceNode = infoContainer.SelectSingleNode("./div[contains(@class, 'product-price')]/span");
            var price = Utils.ParsePrice(priceNode.InnerHtml);

            var image = WebsiteBaseUrl + page.SelectSingleNode("//img[contains(@class, 'primary-image')]")
                            .GetAttributeValue("src", "").Substring(1);

            var details = new ProductDetails
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };

            details.AddSize(productName, "Unknown");
            return details;
        }

        private static async Task<List<HtmlNode>> GetPageTask(List<string> urls, CancellationToken token)
        {
            var res = new List<HtmlNode>();
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);

            var documents = await Task.WhenAll(urls.Select(i => client.GetDocTask(i, token)));
            foreach (var document in documents) res.Add(document.DocumentNode);

            return res;
        }

        private Product GetProduct(HtmlNode item)
        {
            try
            {
                var url = GetUrl(item);
                var name = GetName(item);
                var imageUrl = GetImageUrl(item);
                var price = GetPrice(item);
                var currency = GetCurrency(item);
                return new Product(this, name, url, price, imageUrl, url, currency);
            }
            catch
            {
                return null;
            }
        }

        private static string GetName(HtmlNode item)
        {
            var type = item.SelectSingleNode("./div/div[contains(@class, 'product-cat')]/a").InnerHtml;
            type = Regex.Replace(type, @"\t|\n|\r", "");
            var name = item.SelectSingleNode("./div").GetAttributeValue("data-itemname", "");
            return $"{type} - {name}";
        }

        private static string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div[contains(@class, 'product-image')]/a")
                .GetAttributeValue("href", "");
        }

        private string GetImageUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode("./div/div[contains(@class, 'product-image')]/a/img")
                       .GetAttributeValue("src", "");
        }

        private static double GetPrice(HtmlNode item)
        {
            var priceContainer = item.SelectSingleNode("./div").GetAttributeValue("data-itemprice", "");
            if (!double.TryParse(priceContainer, out var res)) throw new Exception();

            return res;
        }

        private static string GetCurrency(HtmlNode item)
        {
            return "€";
        }
    }
}