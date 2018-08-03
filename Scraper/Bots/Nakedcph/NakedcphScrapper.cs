using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Nakedcph
{
    [Serializable]
    public class NakedcphScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Nakedcph";
        public override string WebsiteBaseUrl { get; set; } = "https://www.nakedcph.com";
        public override bool Active { get; set; }


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                $"https://www.nakedcph.com/search/searchbytext/{settings.KeyWords}/1?orderBy=Published";
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.FireFoxHeaders);
            var document = request.GetDoc(searchUrl, token);
            Logger.Instance.WriteErrorLog("Unexpected html!");
            var nodes = document.DocumentNode.SelectSingleNode("//*[@id='products']");
            HtmlNodeCollection children = nodes.SelectNodes("./div");

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child);
#else
                LoadSingleProductTryCatchWraper(listOfProducts,child);
#endif
            }

        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="info"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child);
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
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child)
        {
            string priceStr = child.SelectSingleNode(".//span[contains(@class, 'price')]/del").InnerText;

            var urlNode = child.SelectSingleNode("./a");
            string productURL = new Uri(new Uri(this.WebsiteBaseUrl), urlNode.GetAttributeValue("href", null)).ToString();   
            double price = changeStrIntoDouble(priceStr);
            var productName = child.SelectSingleNode(".//span[contains(@class, 'product-name d-block')]").InnerText;
            var image = child.SelectSingleNode(".//img[contains(@class,'card-img-top')]");
            string imageURL = new Uri(new Uri(this.WebsiteBaseUrl), image.GetAttributeValue("data-src", null)).ToString();  

            Product product = new Product(this, productName, productURL, price, imageURL, productURL);
            listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            const string xpath = "//*[@id='product-form']//div[contains(@class,'dropdown-menu')]/a";
            using (var client = ClientFactory.GetProxiedClient().AddHeaders(ClientFactory.FireFoxHeaders))
            {
                var doc = client.GetDoc(product.Url, token);

                var nodes = doc.DocumentNode.SelectNodes(xpath);
                var sizes = nodes.Select(node => node.InnerText.Trim());
                return new ProductDetails() { SizesList = sizes.ToList() };
            }
        }
    }
}
