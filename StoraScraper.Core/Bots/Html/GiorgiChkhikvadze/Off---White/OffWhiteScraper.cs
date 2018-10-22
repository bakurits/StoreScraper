using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.CookieCollecting;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.GiorgiChkhikvadze
{
    [Serializable]
    public class OffWhiteScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; } = "Off---white";
        public sealed override string WebsiteBaseUrl { get; set; } = "https://www.off---white.com";

        private const string SearchUrlFormat = @"https://www.off---white.com/en/US/search?q={0}";

        public override bool Active { get; set; }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {

            listOfProducts = new List<Product>();

            var searchUrl = string.Format(SearchUrlFormat, settings.KeyWords);
            FindItemsInternal(listOfProducts, settings, token, searchUrl);
        }



        private void FindItemsInternal(List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token, string url)
        {

            HtmlDocument document = new HtmlDocument();
            var client = ClientFactory.GetProxiedFirefoxClient();
            var response = CfBypasser.GetRequestedPage(client, this, url, token);

            document.LoadHtml(response.Content.ReadAsStringAsync().Result);

            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to off---white");
                throw new WebException("Can't connect to website");
            }

            var node = document.DocumentNode;
            var container = node.SelectSingleNode("//section[@class='products']");

            if (container == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected Html!!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexpected Html");
            }

            var items = container.SelectNodes("./article");



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


        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings,
            HtmlNode item)
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

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="item"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            var url = "https://www.off---white.com" + item.SelectSingleNode("./a").GetAttributeValue("href", "");
            string name = item.SelectSingleNode("./a/figure/figcaption/div").InnerText;
            var priceNode = item.SelectSingleNode("./a/figure/figcaption/div[4]/span[1]/strong");

            var price = Utils.ParsePrice(priceNode.InnerText);

            string imagePath = item.SelectSingleNode("./a/figure/img").GetAttributeValue("src", null);
            if (imagePath == null)
            {
                Logger.Instance.WriteErrorLog("Image Of product couldn't found");
            }

            string id = item.GetAttributeValue("data-json-url", null);


            Product p = new Product(this, name, url, price.Value, imagePath, id, price.Currency);
            if (!Utils.SatisfiesCriteria(p, settings)) return;

            p.ImageUrl = imagePath;

            listOfProducts.Add(p);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            HtmlDocument doc = new HtmlDocument();
            var client = ClientFactory.GetProxiedFirefoxClient();
            var response = CfBypasser.GetRequestedPage(client, this, productUrl, token);

            doc.LoadHtml(response.Content.ReadAsStringAsync().Result);
            var root = doc.DocumentNode;
            var sizeNodes = root.SelectNodes("//*[contains(@class,'styled-radio')]/label");
            var sizes = sizeNodes.Select(node => node.InnerText).ToList();

            var name = root.SelectSingleNode("//*[contains(@class, 'prod-title')]").InnerText.Trim();
            var priceNode = root.SelectSingleNode("//div[contains(@class, 'price')]/span/strong");
            var price = Utils.ParsePrice(priceNode.InnerText);
            var image = root.SelectSingleNode("//*[@id='image-0']").GetAttributeValue("src", null);

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

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            FindItemsInternal(listOfProducts, null, token, "https://www.off---white.com/en/US/section/new-arrivals");
        }
    }
}
