using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Flurl.Http;
using HtmlAgilityPack;
using StoreScraper.Browser;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mrporter
{
    public class MrporterScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Mrporter";
        public override string WebsiteBaseUrl { get; set; } = "https://www.mrporter.com/";
        public override Type SearchSettings { get; set; } = typeof(MrporterSearchSettings);
        public override bool Enabled { get; set; }


        private const string SearchUrlFormat = @"https://www.mrporter.com/mens/whats-new";

        /// <summary>
        /// This method is for finding items on page
        /// With given restrictions 
        /// </summary>
        /// <param name="listOfProducts">List of Products which fits in restrictions</param>
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settingsObj,
            CancellationToken token, Logger info)
        {

            var settings = (MrporterSearchSettings)settingsObj;
            listOfProducts = new List<Product>();

            var searchUrl = SearchUrlFormat;
            var node = GetPage(searchUrl, token);
            
            Worker(listOfProducts, settings, node, token);
                   
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
                if (dataStock == "Low_Stock" || dataStock == "In_Stock")
                {
                    string val = GenerateRealSize(item.InnerHtml, sizeCaster);

                    Debug.WriteLine(val);
                    result.Add(val);
                }
                
            }

            return result;
        }

        private string GenerateRealSize(string html, string caster)
        {
            string result = html;
            string before = html.Substring(0, html.IndexOf("-", StringComparison.Ordinal)).Trim();
            int val;
            if (int.TryParse(before, out val))
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


        private HtmlNode GetPage(string url, CancellationToken token)
        {
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.ChromeHeaders);
            var document = request.GetDoc(url, token);
            return document.DocumentNode;
        }


        private void Worker(List<Product> listOfProducts, MrporterSearchSettings settings, HtmlNode node, CancellationToken token)
        {
            HtmlNodeCollection collection =
                node.SelectNodes(
                    "//div[@class='product-details']/div[@class='description']|//div[@class='product-details']/div[@class='description description-last']");

            foreach (var htmlNode in collection)
            { 
                string url = htmlNode.SelectSingleNode("./div/a").GetAttributeValue("href", null);
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
                    foreach (var keyWord in keyWordSplit)
                    {
                        if (curProduct.Name.ToLower().Contains(keyWord.ToLower()))
                        {
                            listOfProducts.Add(curProduct);
                            break;
                        }       
                    }
                    
                }

                token.ThrowIfCancellationRequested();
            }
        }

    }
}