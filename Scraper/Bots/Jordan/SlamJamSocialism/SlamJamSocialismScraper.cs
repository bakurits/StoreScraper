using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Jordan.SlamJamSocialism
{
    public class SlamJamSocialismScraper: ScraperBase
    {
        public override string WebsiteName { get; set; } = "SlamJamSocialism";
        public override string WebsiteBaseUrl { get; set; } = "https://slamjamsocialism.com/";
        public override bool Active { get; set; }
        
        private const string SearchUrl = @"https://www.slamjamsocialism.com/module/ambjolisearch/jolisearch?search_query={0}";
        
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var keywordUrl = String.Format(SearchUrl, settings.KeyWords);
            var pageOne = GetWebpage(keywordUrl, token);
            var firstResults = pageOne.SelectNodes("//div[contains(@class, 'product-container')]");
            
          
            
            foreach (var item in firstResults)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(listOfProducts, settings, item, token);
            }

         

            var temp = pageOne;
            
            while(true)
            {
                var nextPage = temp.SelectSingleNode("//li[contains(@id, 'pagination_next_bottom')]/a").GetAttributeValue("href", null);
               
                if (nextPage == null)
                {
                    return;
                }
                var page = GetWebpage("https://slamjamsocialism.com"+nextPage, token);
                temp = page;
                var results =  page.SelectNodes("//div[contains(@class, 'product-container')]");
                foreach (var item in results)
                {
                    token.ThrowIfCancellationRequested();
                    LoadSingleProduct(listOfProducts, settings, item, token);
                }
                
            }
            
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item, CancellationToken token)
        {
            
            Debug.WriteLine(item.OuterHtml);
           
            string name = GetName(item);
            string url = GetUrl(item);
            double price = GetPrice(item);
            string imgurl = GetImg(item);
            string currency = GetCurrency(item);
            var product = new Product(this, name, url, price, imgurl, url, currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
         
        }

        private string GetCurrency(HtmlNode item)
        {
            return item.SelectSingleNode("//div[@class='content_price'][1]/meta[1]").GetAttributeValue("content", null);
        }

        private string GetImg(HtmlNode item)
        {
            return item.SelectSingleNode("//img[@class='replace-2x img-responsive lazyload'][1]").GetAttributeValue("src", null);
        }

        private double ParsePrice(string pricee)
        {
            
            string result = Regex.Match(pricee, @"[\d\.]+").Value;
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }

        private double GetPrice(HtmlNode item)
        {
            return ParsePrice(item.SelectSingleNode("//div[@class='content_price'][1]/span[@class='price product-price'][1]").InnerHtml);

        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("//a[@class='product_img_link'][1]").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("//a[@class='product_img_link'][1]").GetAttributeValue("href", null);
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;
        }


        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            var toSearch = String.Format(SearchUrl, settings.KeyWords);
            var searchResults = GetWebpage(toSearch, token);
            return searchResults.SelectNodes("//a[contains(@class, 'product')]");
            
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}