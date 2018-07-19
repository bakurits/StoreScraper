using System;
using System.Collections.Generic;
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
        public override Type SearchSettings { get; set; } = typeof(SearchSettingsBase);
        public override bool Enabled { get; set; }
       
        public override void FindItems(out List<Product> listOfProducts, object settings, CancellationToken token, Logger info)
        {
            var setings = (SearchSettingsBase) settings;      
            listOfProducts = new List<Product>();
            String searchUrl = $"https://www.footaction.com/api/products/search?currentPage=1&pageSize=60&query={setings.KeyWords}";
            var request = searchUrl.WithProxy().WithHeaders(ClientFactory.ChromeHeaders);
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
            foreach (XmlNode  singleContact in products)
            {
                XmlNode personalNode = singleContact.SelectSingleNode("name");
                string productName = personalNode?.InnerText;
                string imageUrl = singleContact.SelectSingleNode("images/url")?.InnerText;
                double.TryParse(singleContact.SelectSingleNode("price/value")?.InnerText,out var price);
                string sku = singleContact.SelectSingleNode("sku")?.InnerText;
                string link = new Uri($"https://www.footaction.com/product/{productName}/{sku}.html").AbsoluteUri;
                var product = new Product(this.WebsiteName,productName,link,price,sku,imageUrl);
                if (Utils.SatisfiesCriteria(product,setings))
                    listOfProducts.Add(product);
            }
        }

    }
}