using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Xml;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Footaction
{
    [Serializable]
    public class FootactionScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Footaction";
        public override string WebsiteBaseUrl { get; set; } = "https://www.footaction.com";
        public override bool Active { get; set; }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token, Logger info)
        {
            listOfProducts = new List<Product>();
            string searchUrl =
                $"https://www.footaction.com/api/products/search?currentPage=0&pageSize=50&query={settings.KeyWords}&sort=newArrivals";
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.FireFoxHeaders);
            var task = request.GetStringAsync(searchUrl);
            task.Wait(token);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(task.Result);
            var products = xmlDocument.SelectNodes("productCategorySearchPage/products");
            if (products == null)
            {
                info.WriteLog("[Error] Uncexpected XML!!");
                throw new WebException("Undexpected XML");
            }

            int sum = 0;

            foreach (XmlNode singleContact in products)
            {
                token.ThrowIfCancellationRequested();
                if (sum > 50) break;
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, singleContact, ref sum);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, singleContact, ref sum, info);
#endif
            }
        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="singleContact"></param>
        /// <param name="sum"></param>
        /// <param name="info"></param>
        private void LoadSingleProductTryCatchWraper(List<Product>listOfProducts, SearchSettingsBase settings, XmlNode singleContact, ref int sum, Logger info)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, singleContact, ref sum);
            }
            catch (Exception e)
            {
                info.WriteLog(e.Message);
            }

        }

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="singleContact"></param>
        /// <param name="sum"></param>
        private void LoadSingleProduct(List<Product>listOfProducts, SearchSettingsBase settings, XmlNode singleContact, ref int sum)
        {
            ++sum;
            XmlNode personalNode = singleContact.SelectSingleNode("name");
            string productName = personalNode?.InnerText;
            string imageUrl = singleContact.SelectSingleNode("images/url")?.InnerText;
            double.TryParse(singleContact.SelectSingleNode("price/value")?.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
            string sku = singleContact.SelectSingleNode("sku")?.InnerText;
            string link = new Uri($"https://www.footaction.com/product/{productName}/{sku}.html").AbsoluteUri;
            var product = new Product(this.WebsiteName, productName, link, price, sku, imageUrl);
            if (Utils.SatisfiesCriteria(product, settings))
                listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}