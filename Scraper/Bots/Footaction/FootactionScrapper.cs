using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
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
            
            throw new NotImplementedException();
        }

    }
}