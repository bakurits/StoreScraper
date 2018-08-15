using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using StoreScraper;
using StoreScraper.Attributes;
using StoreScraper.Bots.Bakurits.Antonioli;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace ScraperCore.Bots.Sticky_bit.EastBay_FootAction
{
   [DisabledScraper]
    public class FootAPIBase : ScraperBase
    {

        public override string WebsiteName { get; set; }
        public override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }
        public override Type SearchSettingsType { get; set; } = typeof(FootApiSearchSettings);

        public FootAPIBase(string websiteName, string websiteBaseUrl)
        {
            this.WebsiteName = websiteName;
            this.WebsiteBaseUrl = websiteBaseUrl;
        }


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            FootApiSearchSettings.GenderEnum gender = ((FootApiSearchSettings) settings).Gender;
            try
            {
                gender = ((FootApiSearchSettings)settings).Gender;
            }
            catch
            {
                gender = FootApiSearchSettings.GenderEnum.Any;
            }
            string searchUrl = WebsiteBaseUrl + $"/api/products/search?currentPage=0&pageSize=50&sort=newArrivals&query={settings.KeyWords}%3Arelevance";
            if (gender != FootApiSearchSettings.GenderEnum.Any)
                searchUrl += $"3Agender%{gender.GetDescription()}";
            using (var client = ClientFactory.CreateProxiedHttpClient().AddHeaders(ClientFactory.JsonXmlAcceptHeader))
            {
                var responseText = client.GetAsync(searchUrl, token).Result.Content.ReadAsStringAsync().Result;
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(responseText);
                var products = xmlDocument.SelectNodes("productCategorySearchPage/products");
                if (products == null)
                {
                    Logger.Instance.WriteVerboseLog("[Error] Uncexpected XML!!");
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
                LoadSingleProductTryCatchWraper(listOfProducts, settings, singleContact, ref sum);
#endif
                }
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
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings,
            XmlNode singleContact, ref int sum)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, singleContact, ref sum);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteVerboseLog(e.Message);
            }

        }

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="singleContact"></param>
        /// <param name="sum"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, XmlNode singleContact,
            ref int sum)
        {
            ++sum;
            XmlNode personalNode = singleContact.SelectSingleNode("name");
            string productName = personalNode?.InnerText;
            string imageUrl = singleContact.SelectSingleNode("images/url")?.InnerText;
            double.TryParse(singleContact.SelectSingleNode("price/value")?.InnerText, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var price);
            string sku = singleContact.SelectSingleNode("sku")?.InnerText;
            string link = new Uri(WebsiteBaseUrl + $"/product/{productName}/{sku}.html").AbsoluteUri;

            var product = new Product(this, productName, link, price, imageUrl, sku);
            listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            return new ProductDetails();
        }

        public class FootLockerScraper : FootAPIBase
        {
            public FootLockerScraper() : base("FootLocker", "https://www.footlocker.com")
            {
            }

        }

        public class FootActionScraper : FootAPIBase
        {
            public FootActionScraper() : base("FootAction", "https://www.footaction.com")
            {
            }
        }
    }
}
