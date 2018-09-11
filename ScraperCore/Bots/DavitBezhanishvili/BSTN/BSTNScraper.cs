using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Sticky_bit.BSTN
{
    public class BSTNScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }

        private string SearchPrefix = @"/en/search/";
        private const string Keywords = @"{0}";
        private string SearchSuffix = @"/page/1/sort/date_new";

        private const string UlXpath = @"//*[@class=""block-grid four-up mobile-two-up productlist""]";


        public BSTNScraper()
        {
            this.WebsiteName = "BSTN";
            this.WebsiteBaseUrl = "http://www.bstn.com";
            this.Active = true;
        }


        private HtmlDocument GetWebPage(string url, CancellationToken token)
        {
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(url, token);

            return document;
        }

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var url = "";
            Scrap(url, ref listOfProducts, null, token);
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl = WebsiteBaseUrl + SearchPrefix + string.Format(Keywords, settings.KeyWords) + SearchSuffix;
            Scrap(searchUrl, ref listOfProducts, settings, token);
        }

        private void Scrap(string url, ref List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            var node = GetWebPage(url, token).DocumentNode;
            var children = node.SelectNodes("//li[@class='item']");

            foreach (var child in children)
                if (child.SelectSingleNode(".//div[@class='pLabel comingsoon']") == null && child.GetAttributeValue("style", null) == null)
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
            var client = ClientFactory.GetProxiedFirefoxClient();
            var node = client.GetDoc(productUrl, token)
                .DocumentNode;
            HtmlNodeCollection sizes = node.SelectNodes("//*[@class=\"product_sizes\"]//*[@class=\"button\"]");
            ProductDetails details = new ProductDetails();
            foreach (var s in sizes.Select(size => size.InnerText.Trim()))
            {
                details.AddSize(s, "Unknown");
            }

            return details;
        }
    }
}
