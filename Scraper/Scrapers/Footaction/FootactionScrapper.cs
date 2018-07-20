using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Xml;
using Flurl;
using StoreScraper.Factory;
using StoreScraper.Interfaces;
using StoreScraper.Models;
using Flurl.Http;
using StoreScraper;
using StoreScraper.Helpers;

namespace StoreScraper.Bots.Footaction
{
    [Serializable]
    public class FootactionScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Footaction";
        public override string WebsiteBaseUrl { get; set; } = "https://www.footaction.com";
        public override bool IsMonitored { get; set; }
       
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, Logger info)
        {
            listOfProducts = new List<Product>();
            String searchUrl = $"https://www.footaction.com/api/products/search?currentPage=0&pageSize=50&query={settings.KeyWords}&sort=newArrivals";
            var request = searchUrl.WithHeaders(ClientFactory.HeaderOnlyJsonXml);
            var task = request.GetStringAsync(token);
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
            foreach (XmlNode  singleContact in products)
            {
                token.ThrowIfCancellationRequested();
                if (sum > 50) break;
                ++sum; 
                XmlNode personalNode = singleContact.SelectSingleNode("name");
                string productName = personalNode?.InnerText;
                string imageUrl = singleContact.SelectSingleNode("images/url")?.InnerText;
                double.TryParse(singleContact.SelectSingleNode("price/value")?.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
                string sku = singleContact.SelectSingleNode("sku")?.InnerText;
                string link = new Uri($"https://www.footaction.com/product/{productName}/{sku}.html").AbsoluteUri;
                if (!settings.LoadImages) imageUrl = null;
                var product = new Product(this.WebsiteName,productName,link,price,sku,imageUrl);
                if (Utils.SatisfiesCriteria(product,settings))
                    listOfProducts.Add(product);
            }
        }

        public override ProductDetails GetProductDetails(string productUrl)
        {
            throw new NotImplementedException();
        }
    }
}