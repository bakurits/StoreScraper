using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.DavitBezhanishvili.AwLab
{
    public class AwLabScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Aw-lab";
        public override string WebsiteBaseUrl { get; set; } = "http://en.aw-lab.com";
        public override bool Active { get; set; }

        private readonly List<String> NewArrivalPageUrls = new List<string>
        {
           "https://en.aw-lab.com/shop/men/new-now/#%2Fshop%2Fmen%2Fnew-now%3Fdir%3Ddesc%26order%3Daw_created_at",
           "https://en.aw-lab.com/shop/women/new-now/#%2Fshop%2Fwomen%2Fnew-now%3Fdir%3Ddesc%26order%3Daw_created_at",
           "https://en.aw-lab.com/shop/shoes-kids/new-now#%2Fshop%2Fshoes-kids%2Fnew-now%3Fdir%3Ddesc%26is_ajax%3D1%26order%3Daw_created_at"
        };

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            Task.WhenAll(NewArrivalPageUrls.Select(url => Scrap(url, data, null, token))).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }

        private async Task Scrap(string url, ConcurrentDictionary<Product, byte> data, SearchSettingsBase settings, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = (await client.GetDocTask(url, token));
            //  Logger.Instance.WriteErrorLog("Unexpected html!");
            var nodes = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'products-grid row')]");
            if (nodes == null)
            {
                Logger.Instance.WriteErrorLog("Unexcepted Html");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexcepted Html");
            }
            var children = nodes.SelectNodes("./div");
            if (children == null)
            {
                return;
            }

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(data, child, settings);
#else
                LoadSingleProductTryCatchWraper(data, child, settings);
#endif
            }
            
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            var searchUrl =
                $"http://en.aw-lab.com/shop/catalogsearch/result/index/q/{settings.KeyWords}";
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            Scrap(searchUrl, data, settings, token).Wait(token);

            listOfProducts = new List<Product>(data.Keys);
        }


        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="settings"></param>
        private void LoadSingleProductTryCatchWraper(ConcurrentDictionary<Product, byte> data, HtmlNode child, SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(data, child, settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private string getImageUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class, 'product-image')]/a/img").GetAttributeValue("src", null);
        }

        private string getProductUrl(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class, 'product-image')]/a").GetAttributeValue("href", null);
        }


        private string getProductName(HtmlNode child)
        {
            return child.SelectSingleNode(".//div[contains(@class,product-data-container)]/h2/a").InnerText;
        }


        private void LoadSingleProduct(ConcurrentDictionary<Product, byte> data, HtmlNode child, SearchSettingsBase settings)
        {

            var imageUrl = getImageUrl(child);
            var productUrl = getProductUrl(child);
            var productName = getProductName(child);
            var priceNode = child.SelectSingleNode(".//*[contains(@class, 'small-price')]");

            var priceStr = priceNode.SelectSingleNode("./del") != null
                ? priceNode.SelectSingleNode("./text()").InnerText
                : priceNode.InnerText;
            Price p = Utils.ParsePrice(priceStr);
            var product = new Product(this, productName, productUrl, p.Value, imageUrl, productUrl, p.Currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                data.TryAdd(product,0);
            }
        }

       
        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var webPage = GetWebpage(productUrl, token);
            var jsonStr = Regex.Match(webPage.InnerHtml, @"var spConfig = new Product.Config\((.*)\)").Groups[1].Value;
            JObject parsed = JObject.Parse(jsonStr);

            ProductDetails details = ConstructProduct(webPage, productUrl);

            var sizes = parsed.SelectToken("attributes").SelectToken("959").SelectToken("options");
            foreach (JToken sz in sizes.Children())
            {
                var sizeName = (string)sz.SelectToken("label");
                var size = parseSize(sizeName);
                details.AddSize(size, "Unknown");
            }
            return details;
        }

        private ProductDetails ConstructProduct(HtmlNode webPage, string productUrl)
        {
            var name = webPage.SelectSingleNode("//div[contains(@class, 'product-name abtest--off')]/h1").InnerText;
            var image = webPage.SelectSingleNode(
                    "//div[contains(@class,'product-img-box')]/p/a/img")
                .GetAttributeValue("src", null);
            var priceNode = webPage.SelectSingleNode("//div[contains(@class,'price-box')]/p[@class='product-price__container']");
            var txt = priceNode.InnerText.Trim().Replace(',', '.');
            var price = Utils.ParsePrice(txt);

            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };
            return details;
        }

        private string parseSize(string sizeName)
        {
            var sizes = sizeName.Split(' ');
            if (sizes.Length == 1) return sizes[0];
            var match = Regex.Match(sizes[1], @"([\d])");
            var numerator = match.Value;
            var denominator = match.NextMatch().Value;

            return sizes[0] + " " + numerator + "/" + denominator;
        }
    }
}
