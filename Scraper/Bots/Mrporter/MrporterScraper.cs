using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Browser;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Scrapers.Mrporter
{
    public class MrporterScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Mrporter";
        public override string WebsiteBaseUrl { get; set; } = "https://www.mrporter.com/";
        public override Type SearchSettings { get; set; } = typeof(SearchSettingsBase);
        public override bool Active { get; set; }


        private const string SearchUrlFormat = @"https://www.mrporter.com/mens/whats-new";
        private HttpClient client = ClientFactory.GetHttpClient(autoCookies: true).AddHeaders(ClientFactory.FireFoxHeaders);

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token, Logger info)
        {
            listOfProducts = new List<Product>();

            var searchUrl = SearchUrlFormat;
            var node = GetPage(searchUrl, token, info);
            
            Worker(listOfProducts, settings, node, token, info);
                   
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            ProductDetails result = new ProductDetails();
            var doc = GetPage(product.Url, token);
            var node = doc.SelectSingleNode("//select[contains(@class, 'select-option-style')]");

            string sizeCaster = "";
            try
            {
                sizeCaster = doc.SelectSingleNode("//ul[contains(@class, 'product-accordion__list')]").SelectSingleNode("./li").InnerHtml;
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
                string val = GenerateRealSize(item.InnerHtml, sizeCaster);

                Debug.WriteLine(val);
                result.Add(val);

            }

            return result;
        }

        private string GenerateRealSize(string html, string caster)
        {
            string result = html;
            int ind = html.IndexOf("-", StringComparison.Ordinal);
            string before = html.Substring(0,  ind != -1 ? ind : html.Length).Trim();
            if (int.TryParse(before, out var val))
            {
                int indx = caster.IndexOf(val.ToString(), comparisonType: StringComparison.Ordinal);
                if (indx != -1)
                {
                    int indOfEqualitySign = caster.IndexOf("=", indx, StringComparison.Ordinal);
                    int indOfTokenFinish = caster.IndexOf(",", indOfEqualitySign, StringComparison.Ordinal);
                    if (indOfTokenFinish == -1) indOfTokenFinish = caster.Length;
                    result = caster.Substring(indOfEqualitySign + 1, indOfTokenFinish - indOfEqualitySign - 1).Trim();
                    result += html.Substring(html.IndexOf("-", StringComparison.Ordinal) - 1);
                }
            }
            return result;
        }


        private HtmlNode GetPage(string url, CancellationToken token, Logger logger = null)
        {
            var document = client.GetDoc(url, token, logger);
            return document.DocumentNode;
        }


        private void Worker(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode node, CancellationToken token, Logger info)
        {
            HtmlNodeCollection collection =
                node.SelectNodes(
                    "//div[@class='product-details']/div[@class='description']|//div[@class='product-details']/div[@class='description description-last']");

            foreach (var htmlNode in collection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, htmlNode);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, htmlNode, info);
#endif

            }
        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="htmlNode"></param>
        /// <param name="info"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode htmlNode, Logger info)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, htmlNode);
            }
            catch (Exception e)
            {
                info.WriteLog(e.Message);
            }
        }

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="htmlNode"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode htmlNode)
        {
            string aHref = htmlNode.SelectSingleNode("./div/a").GetAttributeValue("href", null);
            string url = "https://www.mrporter.com/" + aHref.Substring(aHref.IndexOf("/", 1, StringComparison.Ordinal) + 1);

            string name = htmlNode.SelectSingleNode("./div/a/span[1]").InnerHtml + " " + htmlNode.SelectSingleNode("./div/a/span[2]").InnerHtml;

            var priceContainer = htmlNode.SelectSingleNode("./div[@class='price-container']");

            var newPrice = priceContainer.SelectSingleNode(".//span[@class='price-value']");
            double price = 0;
            if (newPrice != null)
            {
                string html = newPrice.InnerHtml;
                html = html.Substring(html.LastIndexOf("&pound;", StringComparison.Ordinal) + 7);
                price = Convert.ToDouble(html);
            }
            else
            {
                price = Convert.ToDouble(priceContainer.SelectSingleNode("./p[1]").InnerHtml.Substring(7));
            }

            Product curProduct = new Product(this.WebsiteName, name, url, price, url, null);

            if (Utils.SatisfiesCriteria(curProduct, settings))
            {
                var keyWordSplit = settings.KeyWords.Split(' ');
                if (keyWordSplit.All(keyWord => curProduct.Name.ToLower().Contains(keyWord.ToLower())))
                {
                    listOfProducts.Add(curProduct);
                }
            }
        }

    }
}