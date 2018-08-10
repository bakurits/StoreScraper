using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Http;
using StoreScraper.Models;

#pragma warning disable 4014

namespace StoreScraper.Bots.Bakurits.Mrporter
{
    public class MrporterScraper : ScraperBase
    {
        private const string SearchUrlFormat = @"https://www.mrporter.com/mens/whats-new";


        private bool _active;
        public override string WebsiteName { get; set; } = "Mrporter";
        public override string WebsiteBaseUrl { get; set; } = "https://www.mrporter.com/";

        public override bool Active
        {
            get => _active;
            set
            {
                if (!_active && value)
                {
                    CookieCollector.Default.RegisterActionAsync(WebsiteName,
                        (httpClient, token) => httpClient.GetAsync("https://www.mrporter.com/mens/whats-new"),
                        TimeSpan.FromMinutes(20)).Wait();
                    _active = true;
                }
                else if (_active && !value)
                {
                    CookieCollector.Default.RemoveAction(WebsiteName);
                    _active = false;
                }
            }
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();

            var searchUrl = SearchUrlFormat;
            var node = GetPage(searchUrl, token);

            Worker(listOfProducts, settings, node, token);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var result = new ProductDetails();
            var doc = GetPage(product.Url, token);
            var node = doc.SelectSingleNode("//select[contains(@class, 'select-option-style')]");

            var sizeCaster = "";
            try
            {
                sizeCaster = doc.SelectSingleNode("//ul[contains(@class, 'product-accordion__list')]")
                    .SelectSingleNode("./li").InnerHtml;
            }
            catch
            {
                // ignored
            }

            var optionList = node.SelectNodes("./option");

            foreach (var item in optionList)
            {
                var dataStock = item.GetAttributeValue("data-stock", null);
                if (dataStock != "Low_Stock" && dataStock != "In_Stock") continue;
                GenerateRealSize(result, item.InnerHtml, sizeCaster);
            }

            return result;
        }

        private void GenerateRealSize(ProductDetails resultDetails, string html, string caster)
        {
            var ind = html.IndexOf("-", StringComparison.Ordinal);
            var before = html.Substring(0, ind != -1 ? ind : html.Length).Trim();
            var after = html.Substring(ind != -1 ? ind + 1 : html.Length).Trim();
            after = after.Length > 0 ? after : "Unknown";
            var result = before;
            if (int.TryParse(before, out var val))
            {
                var indx = caster.IndexOf(val.ToString(), StringComparison.Ordinal);
                if (indx == -1)
                {
                    resultDetails.AddSize(before, after);
                    return;
                }
                var indOfEqualitySign = caster.IndexOf("=", indx, StringComparison.Ordinal);
                var indOfTokenFinish = caster.IndexOf(",", indOfEqualitySign, StringComparison.Ordinal);
                if (indOfTokenFinish == -1) indOfTokenFinish = caster.Length;
                result = caster.Substring(indOfEqualitySign + 1, indOfTokenFinish - indOfEqualitySign - 1).Trim();
            }
            resultDetails.AddSize(result, after);
        }


        private HtmlNode GetPage(string url, CancellationToken token)
        {
            var client = _active
                ? CookieCollector.Default.GetClient()
                : ClientFactory.GetProxiedFirefoxClient(autoCookies: true);

            var document = client.GetDoc(url, token);
            return document.DocumentNode;
        }


        private void Worker(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode node,
            CancellationToken token)
        {
            var infoCollection =
                node.SelectNodes(
                    "//div[@class='product-details']/div[@class='description']|//div[@class='product-details']/div[@class='description description-last']");
            var imageCollection = node.SelectNodes("//*[@id='product-list']/div/div/a/img");

            for (var i = 0; i < infoCollection.Count; i++)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, infoCollection, imageCollection, i);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, infoCollection, imageCollection, i);
#endif
            }
        }

        /// <summary>
        ///     This method is simple wrapper on LoadSingleProduct
        ///     To catch all Exceptions during
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="ind"></param>
        /// <param name="infoCollection"></param>
        /// <param name="imageCollection"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings,
            HtmlNodeCollection infoCollection, HtmlNodeCollection imageCollection, int ind)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, infoCollection, imageCollection, ind);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        /// <summary>
        ///     This method handles single product's creation
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="infoCollection"></param>
        /// <param name="imageCollection"></param>
        /// <param name="ind"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings,
            HtmlNodeCollection infoCollection, HtmlNodeCollection imageCollection, int ind)
        {
            var htmlNode = infoCollection[ind];
            var imgNode = imageCollection[ind];
            var aHref = htmlNode.SelectSingleNode("./div/a").GetAttributeValue("href", "");
            var imgSrc = imgNode.GetAttributeValue("src", "");
            var url = "https://www.mrporter.com/" +
                      aHref.Substring(aHref.IndexOf("/", 1, StringComparison.Ordinal) + 1);
            var imgUrl = "https:" + imgSrc;

            var name = htmlNode.SelectSingleNode("./div/a/span[1]").InnerHtml + " " +
                       htmlNode.SelectSingleNode("./div/a/span[2]").InnerHtml;

            var priceContainer = htmlNode.SelectSingleNode("./div[@class='price-container']");

            var newPrice = priceContainer.SelectSingleNode(".//span[@class='price-value']");
            double price = 0;
            if (newPrice != null)
            {
                var html = newPrice.InnerHtml;
                price = GetPrice(html);
            }
            else
            {
                price = GetPrice(priceContainer.SelectSingleNode("./p[1]").InnerHtml);
            }

            var curProduct = new Product(this, name, url, price, imgUrl, url, "GBR");

            if (Utils.SatisfiesCriteria(curProduct, settings))
            {
                var keyWordSplit = settings.KeyWords.Split(' ');
                if (keyWordSplit.All(keyWord => curProduct.Name.ToLower().Contains(keyWord.ToLower())))
                    listOfProducts.Add(curProduct);
            }
        }

        private double GetPrice(string html)
        {
            var ind = html.LastIndexOf("&pound;", StringComparison.Ordinal);
            if (ind > -1)
            {
                html = html.Substring(ind + 7);
                return Convert.ToDouble(html);
            }

            var result = Regex.Replace(html, @"[^\d]", "");
            return Convert.ToDouble(result);
        }
    }
}