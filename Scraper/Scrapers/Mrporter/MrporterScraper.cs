﻿using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Browser;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Interfaces;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mrporter
{
    public class MrporterScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Mrporter";
        public override string WebsiteBaseUrl { get; set; } = "https://www.mrporter.com/";
        public override Type SearchSettings { get; set; } = typeof(MrporterSearchSettings);
        public override bool Enabled { get; set; }


        private const string SearchUrlFormat = @"http://www.mrporter.com/mens/whats-new";


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settingsObj, CancellationToken token, Logger info)
        {

            var settings = (MrporterSearchSettings)settingsObj;
            listOfProducts = new List<Product>();

            var node = GetPage(settings.KeyWords, 1, token);
            
            Worker(listOfProducts, settings, node, token);
                   
        }


        private HtmlNode GetPage(string keywords, int page, CancellationToken token)
        {
            var searchUrl = string.Format(SearchUrlFormat);
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.FirefoxHeaders);
            var document = request.GetDoc(searchUrl, token);
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
                            Console.WriteLine(curProduct);
                            break;
                        }       
                    }
                    
                }

                token.ThrowIfCancellationRequested();
            }
        }

    }
}