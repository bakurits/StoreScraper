using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Jordan.SlamJamSocialism
{
    public class SlamJamSocialismScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "SlamJamSocialism";
        public override string WebsiteBaseUrl { get; set; } = "https://www.slamjamsocialism.com/";
        public override bool Active { get; set; }
        
        private const string SearchUrl = @"https://www.slamjamsocialism.com/module/ambjolisearch/jolisearch?search_query={0}";
        private const string NewArrivalsUrl = "https://www.slamjamsocialism.com/new-products?icc=21";

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            Scrap(NewArrivalsUrl, listOfProducts, null, token);
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var keywordUrl = string.Format(SearchUrl, settings.KeyWords);
            Scrap(keywordUrl, listOfProducts, settings, token);

        }

        private void Scrap(string url, List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            var webpage = GetWebpage(url, token);
            var document = webpage.DocumentNode;
            var items = document.SelectNodes("//div[contains(@class, 'product-container')]");

            if (items == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected Html");
                Logger.Instance.SaveHtmlSnapshop(webpage);
                throw new WebException("Unexpected Html");
            }

            foreach (var item in items)
            if(item.SelectSingleNode(".//span[@class='sold-out-label']")==null)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, item, settings);
#else
                LoadSingleProductTryCatchWrapper(listOfProducts, item, settings);
#endif
            }
        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="data"></param>
        /// <param name="child"></param>
        /// <param name="settings"></param>
        private void LoadSingleProductTryCatchWrapper(List<Product> listOProducts, HtmlNode child, SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(listOProducts, child, settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode item, SearchSettingsBase settings)
        {
            var name = GetName(item);
            var url = GetUrl(item);
            var price = GetPrice(item);
            var imgurl = GetImg(item);
            var product = new Product(this, name, url, price.Value, imgurl, url, price.Currency);
            product.BrandName = getBrandName(item);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }

        }

        private string getBrandName(HtmlNode item)
        {
            return item.SelectSingleNode(".//div[contains(@class,'product-item-brand')]").InnerText;
        }

        private string GetImg(HtmlNode item)
        {
            return item.SelectSingleNode(".//a[@class='product_img_link']/img").GetAttributeValue("src", null);
        }


        private Price GetPrice(HtmlNode item)
        {
            return Utils.ParsePrice(item.SelectSingleNode(".//div[contains(@class,'content_price')]/span[@itemprop='price']")
                .InnerText.Trim());

        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode(".//a[@class='product_img_link']").GetAttributeValue("title", null);
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode(".//a[@class='product_img_link']").GetAttributeValue("href", null);
        }

        private HtmlDocument GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token);
        }


        
        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var resp = GetWebpage(productUrl, token).DocumentNode;
            if (resp == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to basketrevolution website");
                throw new WebException("Can't connect to website");
            }
            
            var product = resp.SelectSingleNode("//div[contains(@class, 'primary_block')]");
            var name = product.SelectSingleNode("//h1[@class='h4'][1]").InnerText;
            var price = Utils.ParsePrice(product.SelectSingleNode("//span[@id='our_price_display'][1]").InnerText);
            var imgurl = product.SelectSingleNode("//div[contains(@class,'img-item')][1]/img[contains(@class,'lazyload')][1]").GetAttributeValue("src", null);
            
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

            var sizesStr = Regex.Match(resp.OuterHtml, @"var combinations=(.*?)}}").Groups[1].Value + "}}";
            var sizes = JObject.Parse(sizesStr);
            
            foreach (var size in sizes.Children())
            {
                var sizeVal = size.First.SelectToken("attributes_values").First.First.ToString();
                var quantity = size.First.SelectToken("quantity").ToString();
              
                if(int.Parse(quantity) > 0)
                    result.AddSize(sizeVal, quantity);
            }
            

            return result;

        }
    }
}