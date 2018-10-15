using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Higuhigu.Chmielna
{
    public class ChmielnaScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Chmielna20";
        public override string WebsiteBaseUrl { get; set; } = "https://chmielna20.pl";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://chmielna20.pl/en/products/sneaker/keyword,sneaker/sort,1?keyword={0}";

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            string searchUrl = "https://chmielna20.pl/en/menu/cl20/wszystkie-produkty/page,10";
            var items = GetProductCollection(token, searchUrl);
            

            foreach (var item in items)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, null, item);
#else
                LoadSingleProductTryCatchWrapper(listOfProducts, null, item);
#endif
            }

        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            string url = string.Format(SearchFormat, settings.KeyWords);
            HtmlNodeCollection itemCollection = GetProductCollection(token, url);

            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item);
#else
                LoadSingleProductTryCatchWrapper(listOfProducts, settings, item);
#endif
            }

        }
        
        
        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to chmielna website");
                throw new WebException("Can't connect to website");
            }

            var root = document.DocumentNode;
            var sizeNodes = root.SelectNodes("//div[@class='selector']/ul/li");
            var sizes = sizeNodes?.Select(node => node.GetAttributeValue("data-sizeeu", null) ?? 
                                                  node.GetAttributeValue("data-sizeuk", null) ??
                                                  node.GetAttributeValue("data-value", null)).ToList();

            var name = root.SelectSingleNode("//div[@class='product__name']/h1")?.InnerText.Trim();
            var priceNode = root.SelectSingleNode("//span[@class='product__price_shop']");
            var price = Utils.ParsePrice(priceNode?.InnerText);
            var image = root.SelectSingleNode("//div[@class='item zoom']/img")?.GetAttributeValue("src", null);

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

            Debug.Assert(sizes != null, nameof(sizes) + " != null");
            foreach (var size in sizes)
            {
                result.AddSize(size, "Unknown");
            }

            return result;
        }

        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token);
            return document;
        }

        private HtmlNodeCollection GetProductCollection(CancellationToken token, string url)
        {
            var document = GetWebpage(url, token);
            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to chmielna website");
                throw new WebException("Can't connect to website");
            }
            var node = document.DocumentNode;
            var items = node.SelectNodes("//div[contains(@class, 'products__item')]");
            if (items == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected Html!!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexpected Html");
            }
            return items;
        }

        private void LoadSingleProductTryCatchWrapper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
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
            string priceP = item.SelectSingleNode(".//p[@class='products__item-price']/span[last()]").InnerText;
            return Utils.ParsePrice(priceP);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//img").GetAttributeValue("src", null);
        }
    }
}
