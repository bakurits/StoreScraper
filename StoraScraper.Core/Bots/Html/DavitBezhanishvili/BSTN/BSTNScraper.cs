﻿using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.DavitBezhanishvili.BSTN
{
    public class BSTNScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }


        public BSTNScraper()
        {
            this.WebsiteName = "BSTN";
            this.WebsiteBaseUrl = "https://www.bstn.com";
            this.Active = true;
        }


        private HtmlDocument GetWebPage(string url, CancellationToken token)
        {
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(url, token);

            return document;
        }

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var url = "https://www.bstn.com/en/new-arrivals/page/1/sort/date_new";
            Scrap(url, ref listOfProducts, null, token);
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl = $"https://www.bstn.com/en/search/{settings.KeyWords}/page/1/sort/date_new";
            Scrap(searchUrl, ref listOfProducts, settings, token);
        }

        private void Scrap(string url, ref List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            var node = GetWebPage(url, token).DocumentNode;
            var children = node.SelectNodes("//div[@class='itemWrapper pOverlay']");

            foreach (var child in children)
                if (child.SelectSingleNode(".//div[@class='pLabel comingsoon']") == null &&
                    (child.ParentNode.GetAttributeValue("style", null) == null ||
                     !child.ParentNode.GetAttributeValue("style", null).Equals("visibility:hidden")))
                {
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    LoadSingleProduct(listOfProducts, child, settings);
#else
                    LoadSingleProductTryCatchWraper(listOfProducts, child, settings);
#endif
                }

        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child, settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            var name = child.SelectSingleNode(".//a[@class='pName']").GetAttributeValue("title", null);
            var url = WebsiteBaseUrl + child.SelectSingleNode(".//a[@class='pName']").GetAttributeValue("href", null);
            var priceNode = child.SelectSingleNode(".//div[@class='pText']/span[@class='price']");
            var priceString = (priceNode.SelectSingleNode("./span[@class='newprice']") ?? priceNode).InnerText.Trim();

            Price price = Utils.ParsePrice(priceString);
            var imgUrl = child.SelectSingleNode(".//a[@class = 'plink image']/img").GetAttributeValue("src", null);

            Product product = new Product(this, name, url, price.Value, imgUrl, url, price.Currency);
            listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var webPage = GetWebPage(productUrl, token).DocumentNode;
            ProductDetails details = ConstructProduct(webPage, productUrl);

            var sizesNode = webPage.SelectSingleNode("//div[contains(@class,'selectVariants')]/ul");
            if (sizesNode != null) return OnScreenSizesList(sizesNode, details);
            sizesNode = webPage.SelectSingleNode("//select[@class='customSelectBox']");
            if (sizesNode != null) return DropDownSizesList(sizesNode, details);
            return details;
        }

        private ProductDetails DropDownSizesList(HtmlNode sizesNode, ProductDetails details)
        {
            var sizes = sizesNode.SelectNodes("./option");
            foreach (var size in sizes)
                if (size.GetAttributeValue("class", null) != null && size.GetAttributeValue("class", null) != "disabled")
                {
                    details.AddSize(ExtractEuroSize(size.InnerText.Trim()), "Unknown");
                }
            return details;
        }

        private string ExtractEuroSize(string sizeStr)
        {
            var startInd = sizeStr.IndexOf("EU", StringComparison.Ordinal) + 2;
            var endInd = sizeStr.IndexOf('-');
            return sizeStr.Substring(startInd, endInd - startInd).Trim();
        }

        private ProductDetails OnScreenSizesList(HtmlNode sizesNode, ProductDetails details)
        {
            var sizes = sizesNode.SelectNodes("./li");
            foreach (var size in sizes)
                if (size.SelectSingleNode("./a[contains(@class,'disabled')]") == null)
                {
                    details.AddSize(size.InnerText.Trim(), "Unknown");
                }

            return details;
        }

        private ProductDetails ConstructProduct(HtmlNode webPage, string productUrl)
        {
            var name = webPage.SelectSingleNode("//h1[@itemprop='name']/span[@class='productname']").InnerText.Trim();
            var brand = webPage.SelectSingleNode("//h1[@itemprop='name']/a/span[@class='producer']").InnerText.Trim();
            var image = WebsiteBaseUrl + webPage.SelectSingleNode(
                            "//div[contains(@class,'productSlider')]/ul[@class='slides']/li/a/img").GetAttributeValue("src", null);
            var priceNode = webPage.SelectSingleNode("//div[@class='price']");
            var priceTxt = (priceNode.SelectSingleNode("./span[@class='price']") ?? priceNode.SelectSingleNode("./span[@class='newprice']")).InnerText.Trim();
            var price = Utils.ParsePrice(priceTxt);
            var keyWords = webPage.SelectSingleNode("//meta[@name = 'keywords']").GetAttributeValue("content", null);

            ProductDetails details = new ProductDetails()
            {
                BrandName = brand,
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                KeyWords = keyWords,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };
            return details;
        }
    }
}
