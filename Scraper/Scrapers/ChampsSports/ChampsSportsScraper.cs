using System;
using System.Collections.Generic;
using System.Threading;
using Flurl.Http;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.ChampsSports
{
    public class ChampsSportsScraper : ScraperBase
    {
        public override string WebsiteName { get; set; }
        public override string WebsiteBaseUrl { get; set; }
        public override bool IsMonitored { get; set; }

        private const string UrlPrefix = @"https://www.champssports.com/_-_";
        private const string PageSizeSuffix = @"?cm_PAGE=650&Rpp=650";
        private const string Keywords = @"/keyword-{0}";


        public ChampsSportsScraper()
        {

        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token, Logger info)
        {
            listOfProducts = new List<Product>();

            string searchURL = UrlPrefix + string.Format(Keywords, addKeywords()) + PageSizeSuffix;
            var request = searchURL.WithProxy().WithHeaders(ClientFactory.ChromeHeaders);

            var document = request.GetDoc(token);

            var node = document.DocumentNode;
            HtmlNode container = node.SelectSingleNode("//*[@id=\"endeca_search_results\"]/ul/li[1]");
            Console.WriteLine(container.ToString());
        }

        public override ProductDetails GetProductDetails(string productUrl)
        {
            throw new NotImplementedException();
        }

        public string addKeywords()
        {
            return "blue";
        }
    }
}