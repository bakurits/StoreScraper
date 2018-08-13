using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Jordan.SlamJamSocialism
{
    public class SlamJamSocialismScraper : ScraperBase
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
            string lasstPage;
            try
            {
                 lasstPage = pageOne.SelectSingleNode("//div[contains(@id, 'pagination_bottom')]/ul/li[6]/a/span")
                    .InnerHtml;
            }
            catch (Exception e)
            {
                lasstPage = pageOne.SelectSingleNode("//div[contains(@id, 'pagination_bottom')]/ul/li[5]/a/span").InnerHtml;
            }
           
           
            var stopNumb = int.Parse(lasstPage);
          
            
            foreach (var item in firstResults)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProduct(listOfProducts, settings, item, token);
            }

            

            var temp = pageOne;
            var x = 1;
            while(true)
            {
                x += 1;
                
              
               
                var page = GetWebpage("https://www.slamjamsocialism.com/module/ambjolisearch/jolisearch?search_query="+settings.KeyWords+"&p="+x.ToString(), token);
               
                var results =  page.SelectNodes("//div[contains(@class, 'product-container')]");
                foreach (var item in results)
                {
                    token.ThrowIfCancellationRequested();
                    LoadSingleProduct(listOfProducts, settings, item, token);
                }

                if (x == stopNumb)
                {
                    return;
                }

                
            }
            
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item, CancellationToken token)
        {
            
          
           
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
            return ParsePrice(item.SelectSingleNode("./div//div[@class='content_price'][1]/span[@class='price product-price'][1]").InnerHtml);

        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div/a[@class='product_img_link'][1]").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div/a[@class='product_img_link'][1]").GetAttributeValue("href", null);
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

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var resp = GetWebpage(productUrl, token);
            if (resp == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to basketrevolution website");
                throw new WebException("Can't connect to website");
            }
            
            var product = resp.SelectSingleNode("//div[contains(@class, 'primary_block')]");
            var name = product.SelectSingleNode("//h1[@class='h4'][1]").InnerHtml;
            var price = Utils.ParsePrice(product.SelectSingleNode("//span[@id='our_price_display'][1]").InnerHtml);
            var imgurl = product.SelectSingleNode("//div[contains(@class,'img-item')][1]/img[contains(@class,'lazyload')][1]").GetAttributeValue("src", null);
            var sizes = product.SelectSingleNode("//div[@class='attribute_list']/select[1]");
            var sizesList = sizes.SelectNodes("./option");
            
            ProductDetails result = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = imgurl,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };
            
            foreach (var size in sizesList)
            {
                var sizeString = size.InnerHtml;

                if (sizeString != "Select Size")
                {
                    result.AddSize(sizeString, "Unknown");
                }
            }
            
            return result;

        }
    }
}