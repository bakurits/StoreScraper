using System;
using System.Collections.Generic;
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
        public override bool Enabled { get; set; }
       
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, Logger info)
        {  
            listOfProducts = new List<Product>();
            string searchUrl = $"https://www.footaction.com/api/products/search?currentPage=0&pageSize=50&query={settings.KeyWords}&sort=newArrivals";
            var request = ClientFactory.GetHttpClient().AddHeaders(ClientFactory.JsonXmlAcceptHeader);
            var task = request.GetAsync(searchUrl, token);
            task.Wait(CancellationToken.None);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(task.Result.Content.ReadAsStringAsync().Result);
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

    }
}