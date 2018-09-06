using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.GiorgiChkhikvadze.Nakedcph
{
    
    public class NakedcphScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Nakedcph";
        public override string WebsiteBaseUrl { get; set; } = "http://www.nakedcph.com";
        public override bool Active { get; set; }


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                $"http://www.nakedcph.com/search/searchbytext/{settings.KeyWords}/1?orderBy=Published";
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies:true);
            var document = request.GetDoc(searchUrl, token);
            var nodes = document.DocumentNode.SelectSingleNode("//div[@id='products']");

            if (nodes == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected html!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException();
            }

            HtmlNodeCollection children = nodes.SelectNodes("./div");

            if (children == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected html");
                throw new HtmlWebException("Unexpected html");
            }

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child, settings);
#else
                LoadSingleProductTryCatchWraper(listOfProducts,child,settings);
#endif
            }

        }

        private const string NewArrivalsPageUrl = "https://www.nakedcph.com/new-arrivals/s/6";
        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HttpClient client = ClientFactory.GetProxiedFirefoxClient();
            var doc = client.GetDoc(
                "https://www.nakedcph.com/new-arrivals/s/6/2?orderBy=Published&skip_layout=true&view_override=_ajax-filter",
                token);

            var root = doc.DocumentNode;
            var productNodes = root.SelectNodes("//a[@class='card']");


            IEnumerable<Product> products = GetProducts(productNodes);

        }


        private IEnumerable<Product> GetProducts(HtmlNodeCollection productNodes)
        {
            return from node in productNodes 
                let imageUrlPath = node.SelectSingleNode(".//noscript/img").GetAttributeValue("src", null) 
                let image = imageUrlPath == null ? null : Path.Combine(NewArrivalsPageUrl, imageUrlPath) 
                let name = node.SelectSingleNode(".//h4").InnerText.Trim() let urlPath = node.GetAttributeValue("href", null) 
                let url = urlPath == null? null : Path.Combine(NewArrivalsPageUrl, urlPath) 
                let priceText = node.SelectSingleNode(".//del").InnerText.Trim() 
                let price = Utils.ParsePrice(priceText) 
                select new Product(this, name, url, price.Value, image, url, price.Currency);
        }
      
        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="settings"></param>
  
        private void LoadSingleProductTryCatchWrapper(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child , settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }



        private double changeStrIntoDouble(string priceStr)
        {
            int i = 0;

            for (i = 0; i < priceStr.Length; i++)
            {
                if (!((priceStr[i] >= '0' && priceStr[i] <= '9') || priceStr[i] == '.'))
                {
                    break;
                }
            }

            priceStr = priceStr.Substring(0, i);
            double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            return price;
        }


        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="settings"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            string priceStr = child.SelectSingleNode(".//span[contains(@class, 'price')]/del").InnerText;

            var urlNode = child.SelectSingleNode("./a");
            string productURL = new Uri(new Uri(this.WebsiteBaseUrl), urlNode.GetAttributeValue("href", null)).ToString();   
            double price = changeStrIntoDouble(priceStr);
            var productName = child.SelectSingleNode(".//span[contains(@class, 'product-name d-block')]").InnerText;
            var image = child.SelectSingleNode(".//img[contains(@class,'card-img-top')]");
            string imageURL = new Uri(new Uri(this.WebsiteBaseUrl), image.GetAttributeValue("data-src", null)).ToString();  

            Product product = new Product(this, productName, productURL, price, imageURL, productURL);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            const string xpath = "//*[@id='product-form']//div[contains(@class,'dropdown-menu')]/a";
            var client = ClientFactory.GetProxiedFirefoxClient();
            
            var doc = client.GetDoc(productUrl, token);
            var root = doc.DocumentNode;
        
            var nodes = root.SelectNodes(xpath);
            var sizes = nodes.Select(node => node.InnerText.Trim());
            Regex regex = new Regex(@"([\t]+)");
            var name = root.SelectSingleNode("//*[contains(@class,'product-title')]").InnerText.Trim().Replace("\n","");
            name = regex.Replace(name, " ");
            var priceNode = root.SelectSingleNode("//span[contains(@class, 'price')]/span[2]");
            var price = Utils.ParsePrice(priceNode.InnerText);
            var imageUrl = WebsiteBaseUrl + root.SelectSingleNode("//img[@srcset]").GetAttributeValue("src", "Not Found");

            var details = new ProductDetails()
            {
                Url = productUrl,
                ImageUrl = imageUrl,
                Id = productUrl,
                Name = name,
                Price = price.Value,
                Currency = price.Currency,
                ScrapedBy = this
            };

            foreach (var size in sizes)
            {
                details.AddSize(size, "Unknown");
            }

            return details;
        }
    }
}
