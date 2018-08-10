using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Bakurits.Rimowa
{

    public class RimowaScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Rimowa";
        public override string WebsiteBaseUrl { get; set; } = "https://www.rimowa.com/";
        public int MaxItemCount { get; set; } = 48;
        public override bool Active { get; set; }

        public override Type SearchSettingsType { get; set; } = typeof(SearchSettingsBase);


        private const string Searchformat = @"https://www.rimowa.com/search?q={0}&srule=newest&sz=12&start={1}&format=page-element";
        

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        { 
            listOfProducts = new List<Product>();
            List<String> urls = new List<string>();
            int seenProducts = 0;
            for (int i = 0; seenProducts < MaxItemCount; i++)
            {
                string url = string.Format(Searchformat, settings.KeyWords, i * 12);
                urls.Add(url);
                seenProducts += 12;
            }

            List<Product> res = new List<Product>();
            List<HtmlNode> pages = GetPageTask(urls, token).Result;
            foreach (var page in pages)
            {
                HtmlNodeCollection items = page.SelectNodes("//li[contains(@class, 'grid-tile')]");
                if (items == null) break;
                foreach (var item in items)
                {
                    Product product = GetProduct(item);
                    if (product != null && Utils.SatisfiesCriteria(product, settings))
                    {
                        listOfProducts.Add(product);
                    }
                }
            }
        }


        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private static async Task<List<HtmlNode>> GetPageTask(List<String> urls, CancellationToken token)
        {
            List<HtmlNode> res = new List<HtmlNode>();
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);

            HtmlDocument[] documents = await Task.WhenAll(urls.Select(i => client.GetDocTask(i, token)));
            foreach (var document in documents)
            {
                res.Add(document.DocumentNode);
            }
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

        private string GetName(HtmlNode item)
        {
            string type = item.SelectSingleNode("./div/div[contains(@class, 'product-cat')]/a").InnerHtml;
            type = Regex.Replace(type, @"\t|\n|\r", "");
            string name =  item.SelectSingleNode("./div").GetAttributeValue("data-itemname", "");
            return $"{type} - {name}";
        }
        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div[contains(@class, 'product-image')]/a").GetAttributeValue("href", "");
        }
        private string GetImageUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode("./div/div[contains(@class, 'product-image')]/a/img").GetAttributeValue("src", "");
        }
        private double GetPrice(HtmlNode item)
        {
            string priceContainer = item.SelectSingleNode("./div").GetAttributeValue("data-itemprice", "");
            if (!double.TryParse(priceContainer, out var res))
            {
                throw new Exception();
            }

            return res;
        }
        private string GetCurrency(HtmlNode item)
        {
            return "EUR";
        }
    }
}