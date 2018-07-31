using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
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


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, Logger info)
        {
            listOfProducts = new List<Product>();
            string searchUrl =
                $"https://www.nakedcph.com/search/searchbytext/{settings.KeyWords}/1?orderBy=Published";
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.FireFoxHeaders);
            var task = request.GetStringAsync(searchUrl);
            task.Wait(token);
            var htmlDocument = new HtmlDocument();
            var nodes = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='products']");
            HtmlNodeCollection children = nodes.SelectNodes("./div");

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
                LoadSingleProductTryCatchWraper(listOfProducts,child,info);
            }
        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="info"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child, Logger info)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child);
            }
            catch (Exception e)
            {
                info.WriteLog(e.Message);
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

            string productURL = child.SelectSingleNode("./a").GetAttributeValue("href",null); 
            double price = changeStrIntoDouble(priceStr);
            var productName = child.SelectSingleNode(".//span[contains(@class, 'product-name d-block')]").InnerText;
            var imageURL = child.SelectSingleNode(".//img[contains(@class,'card-img-top embed-responsive-item lazyloaded')]").GetAttributeValue("data-src",null);

            Product product = new Product(this, productName, productURL, price, productURL, imageURL);
            listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
