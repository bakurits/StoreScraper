﻿using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Core;
using StoreScraper.Models;
using System.Text.RegularExpressions;
using System;


namespace StoreScraper.Bots.Sneakers76
{

    public class Sneakers76Scrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Sneakers76";
        public override string WebsiteBaseUrl { get; set; } = "https://www.sneakers76.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
            Console.WriteLine(itemCollection.Count);
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, item);
#endif
            }

        }

        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
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


        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var document = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();

            var sizeCollection = document.SelectNodes("//div[@class='attribute_list']/select/option");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerHtml;
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
                }

            }

            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            //string url = string.Format(SearchFormat, settings.KeyWords);
            string url = WebsiteBaseUrl + "/en/search?search_query="+settings.KeyWords.Replace(" ", "+")+"&search_query="+settings.KeyWords.Replace(" ", "+")+"&orderby=position&orderway=desc&submit_search=&n=336";
            Console.WriteLine(url);
            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//div[@class='product-container product-block']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./div/div/h5/a").InnerHtml.ToLower();
            var validKeywords = settings.KeyWords.ToLower().Split(' ');
            var invalidKeywords = settings.NegKeyWrods.ToLower().Split(' ');
            foreach (var keyword in validKeywords)
            {
                if (!title.Contains(keyword))
                    return false;
            }


            foreach (var keyword in invalidKeywords)
            {
                if (keyword == "")
                    continue;
                if (title.Contains(keyword))
                    return false;
            }


            return true;

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);


            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "EUR");
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private bool GetStatus(HtmlNode item)
        {
            return true;
        }

        private string GetName(HtmlNode item)
        {
            //Console.WriteLine("GetName");
            //Console.WriteLine(item.SelectSingleNode("./a").GetAttributeValue("title", ""));

            return item.SelectSingleNode("./div/div/h5/a").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div/a[@class='product_img_link']").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            string priceDiv = item.SelectSingleNode("./div/div/div[@class='content_price']/span").InnerHtml.Replace("€", "").Replace(",", ".");

            return double.Parse(priceDiv);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div/a[@class='product_img_link']/img").GetAttributeValue("src", null);
        }
    }
}
