﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Browser;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
#pragma warning disable 4014

namespace StoreScraper.Bots.Mrporter
{
    public class MrporterScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Mrporter";
        public override string WebsiteBaseUrl { get; set; } = "https://www.mrporter.com/";
        public override Type SearchSettings { get; set; } = typeof(SearchSettingsBase);


        private bool _active;
        public override bool Active
        {
            get => _active;
            set
            {
                if (!_active && value)
                {
                    CookieCollector.Default.RegisterActionAsync(this.WebsiteName,
                        (httpClient, token) => httpClient.GetAsync("https://www.mrporter.com/mens/whats-new"),
                        TimeSpan.FromMinutes(20)).Wait();
                    _active = true;
                }
                else if(_active && !value)
                {
                    CookieCollector.Default.RemoveAction(this.WebsiteName);
                    _active = false;
                }
            }
        }


        private const string SearchUrlFormat = @"https://www.mrporter.com/mens/whats-new";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();

            string searchUrl = SearchUrlFormat;
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
            string before = html.Substring(0, ind != -1 ? ind : html.Length).Trim();
            if (int.TryParse(before, out var val))
            {
                int indx = caster.IndexOf(val.ToString(), comparisonType: StringComparison.Ordinal);
                if (indx == -1) return result;
                int indOfEqualitySign = caster.IndexOf("=", indx, StringComparison.Ordinal);
                int indOfTokenFinish = caster.IndexOf(",", indOfEqualitySign, StringComparison.Ordinal);
                if (indOfTokenFinish == -1) indOfTokenFinish = caster.Length;
                result = caster.Substring(indOfEqualitySign + 1, indOfTokenFinish - indOfEqualitySign - 1).Trim();
                result += html.Substring(html.IndexOf("-", StringComparison.Ordinal) - 1);
            }
            return result;
        }


        private HtmlNode GetPage(string url, CancellationToken token)
        {
            using (HttpClient client = _active
                ? CookieCollector.Default.GetClient()
                : ClientFactory.GetProxiedClient(autoCookies: true).AddHeaders(ClientFactory.FireFoxHeaders))
            {
                var document = client.GetDoc(url, token);
                return document.DocumentNode;
            }
        }


        private void Worker(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode node, CancellationToken token)
        {
            HtmlNodeCollection infoCollection =
                node.SelectNodes(
                    "//div[@class='product-details']/div[@class='description']|//div[@class='product-details']/div[@class='description description-last']");
            HtmlNodeCollection imageCollection = node.SelectNodes("//*[@id='product-list']/div/div/a/img");

            for (int i = 0; i < infoCollection.Count; i++)
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
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="ind"></param>
        /// <param name="info"></param>
        /// <param name="infoCollection"></param>
        /// <param name="imageCollection"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNodeCollection infoCollection, HtmlNodeCollection imageCollection, int ind)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, infoCollection, imageCollection, ind);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLog(e.Message);
            }
        }

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="infoCollection"></param>
        /// <param name="imageCollection"></param>
        /// <param name="ind"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNodeCollection infoCollection, HtmlNodeCollection imageCollection, int ind)
        {
            var htmlNode = infoCollection[ind];
            var imgNode = imageCollection[ind];
            string aHref = htmlNode.SelectSingleNode("./div/a").GetAttributeValue("href", "");
            string imgSrc = imgNode.GetAttributeValue("src", "");
            string url = "https://www.mrporter.com/" + aHref.Substring(aHref.IndexOf("/", 1, StringComparison.Ordinal) + 1);
            string imgUrl = "https:" + imgSrc;
                
            string name = htmlNode.SelectSingleNode("./div/a/span[1]").InnerHtml + " " + htmlNode.SelectSingleNode("./div/a/span[2]").InnerHtml;

            var priceContainer = htmlNode.SelectSingleNode("./div[@class='price-container']");

            var newPrice = priceContainer.SelectSingleNode(".//span[@class='price-value']");
            double price = 0;
            if (newPrice != null)
            {
                string html = newPrice.InnerHtml;
                price = GetPrice(html);
            }
            else
            {
                price = GetPrice(priceContainer.SelectSingleNode("./p[1]").InnerHtml);
            }

            Product curProduct = new Product(this, name, url, price, url, imgUrl);

            if (Utils.SatisfiesCriteria(curProduct, settings))
            {
                var keyWordSplit = settings.KeyWords.Split(' ');
                if (keyWordSplit.All(keyWord => curProduct.Name.ToLower().Contains(keyWord.ToLower())))
                {
                    listOfProducts.Add(curProduct);
                }
            }
        }

        private double GetPrice(string html)
        {
            int ind = html.LastIndexOf("&pound;", StringComparison.Ordinal);
            if (ind > -1)
            {
                html = html.Substring(ind + 7);
                return Convert.ToDouble(html);
            }
            else
            {
                string result = Regex.Replace(html, @"[^\d]", "");
                return Convert.ToDouble(result);
            }
        }

    }
}