using System;
using System.Collections.Generic;
using System.Linq;
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

#pragma warning disable 4014

namespace StoreScraper.Bots.Html.Bakurits.Mrporter
{
    public class MrporterScraper : ScraperBase
    {
        private const string SearchUrlFormat = @"https://www.mrporter.com/mens/search/{0}?keywords={0}&pn={1}";

        public override string WebsiteName { get; set; } = "Mrporter";
        public override string WebsiteBaseUrl { get; set; } = "https://www.mrporter.com/";

        private int NumberOfPages { get; } = 3;

        public override bool Active { get; set; }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            listOfProducts = new List<Product>();
            var urls = new List<string>();
            for (var i = 1; i <= NumberOfPages; i++) urls.Add(string.Format(SearchUrlFormat, settings.KeyWords, i));

            var products = new List<Product>();
            Task.WhenAll(urls.Select(url => GetItemsForSinglePage(client, url, products, settings, token))).Wait(token);
            listOfProducts = products;
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var doc = GetPage(productUrl, token);
            var nameContainer = doc.SelectSingleNode("//section[contains(@class, 'product-details')]/h1");
            var designer = nameContainer.SelectSingleNode("./a/span/span").InnerHtml.EscapeNewLines();
            var productName = nameContainer.SelectSingleNode("./span/span").InnerHtml.EscapeNewLines();
            var name = designer + "-" + productName;
            var image = WebsiteBaseUrl + doc.SelectSingleNode("//div[contains(@class, 'product-image')]/img")
                            .GetAttributeValue("src", "");
            var priceNode = doc
                .SelectSingleNode("//span[contains(@class, 'product-details-price')]/span/span[@itemprop = 'price']")
                .InnerHtml;
            var price = Utils.ParsePrice(priceNode);
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
                GenerateRealSize(details, item.InnerHtml, sizeCaster);
            }

            return details;
        }

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            var node = GetPage("https://www.mrporter.com/mens/whats-new", token);
            listOfProducts = new List<Product>();
            Worker(listOfProducts, null, node, token);
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
                var index = caster.IndexOf(val.ToString(), StringComparison.Ordinal);
                if (index == -1)
                {
                    resultDetails.AddSize(before, after);
                    return;
                }

                var indOfEqualitySign = caster.IndexOf("=", index, StringComparison.Ordinal);
                var indOfTokenFinish = caster.IndexOf(",", indOfEqualitySign, StringComparison.Ordinal);
                if (indOfTokenFinish == -1) indOfTokenFinish = caster.Length;
                result = caster.Substring(indOfEqualitySign + 1, indOfTokenFinish - indOfEqualitySign - 1).Trim();
            }

            resultDetails.AddSize(result, after);
        }

        private async Task GetItemsForSinglePage(HttpClient client, string url, List<Product> listOfProducts,
            SearchSettingsBase settings,
            CancellationToken token)
        {
            var doc = await client.GetDocTask(url, token);
            Worker(listOfProducts, settings, doc.DocumentNode, token);
        }

        private HtmlNode GetPage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);

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
            if (infoCollection == null || imageCollection == null) return;
            for (var i = 0; i < infoCollection.Count; i++)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, infoCollection, imageCollection, i);
#else
                LoadSingleProductTryCatchWrapper(listOfProducts, settings, infoCollection, imageCollection, i);
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
        private void LoadSingleProductTryCatchWrapper(List<Product> listOfProducts, SearchSettingsBase settings,
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

            var curProduct = new Product(this, name, GetId(url), price, imgUrl, url, "GBR");

            if (settings == null)
            {
                listOfProducts.Add(curProduct);
                return;
            }

            if (Utils.SatisfiesCriteria(curProduct, settings))
                listOfProducts.Add(curProduct);
        }

        private static string GetId(string url)
        {
            int lastIndexOfDash = url.LastIndexOf('/');
            int indexOfQuestionMark = url.LastIndexOf('?');
            if (indexOfQuestionMark == -1) indexOfQuestionMark = url.Length;
            return url.Substr(lastIndexOfDash, indexOfQuestionMark);
        }

        private static double GetPrice(string html)
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