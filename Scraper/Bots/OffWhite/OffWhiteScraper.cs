using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using HtmlAgilityPack;
using StoreScraper.Browser;
using StoreScraper.Controls;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Interfaces;
using StoreScraper.Models;

namespace StoreScraper.Bots.OffWhite
{
    [Serializable]
    public class OffWhiteBot : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Off---white";
        public override string WebsiteBaseUrl { get; set; } = "Off---white.com";

        public override bool Enabled { get; set; }

        [Browsable(false)]
        public List<string> CurrentCart { get; set; } = new List<string>();

        private const string SearchUrlFormat = @"https://www.off---white.com/en/US/search?q={0}";

        public OffWhiteBot()
        {
            CookieCollector.Default.RegisterAction("OffWhite", browser =>
            {
                browser.Navigate().GoToUrl("https://Off---White.com/es/US");

                Task.Delay(6000).Wait();

                return browser.Manage().Cookies.AllCookies;

            }, 30000);
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings
            , CancellationToken token, Logger info)
        {

            listOfProducts = new List<Product>();

            var searchUrl = string.Format(SearchUrlFormat, settings.KeyWords);

            var cookies = CookieCollector.Default.GetCookies("OffWhite", token);
            string proxy = CookieCollector.Default.GetCurrentProxy();
            var request = ClientFactory.GetHttpClient(new WebProxy(proxy)).AddHeaders(ClientFactory.FireFoxHeaders)
                .AddHeaders(("Referer", @"https://www.off---white.com/")).AddCookies(cookies);
            var document = request.GetDoc(searchUrl, token);

            var node = document.DocumentNode;
            HtmlNode container = node.SelectSingleNode("//section[@class='products']");

            if (container == null)
            {
                info.WriteLog("[Error] Uncexpected Html!!");
                throw new WebException("Undexpected Html");
            }

            var items = container.SelectChildren("article");
            

           
            foreach (var item in items)
            {
                
                token.ThrowIfCancellationRequested();
                var url = "https://www.off---white.com" + item.SelectSingleNode("./a").GetAttributeValue("href", "");
                string name = item.SelectSingleNode("./a/figure/figcaption/div").InnerText;
                var priceNode = item.SelectSingleNode("./a/figure/figcaption/div[4]/span[1]/strong");
                bool parseSuccess = double.TryParse(priceNode.InnerText.Substring(2), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var price);

                if(!parseSuccess) throw new WebException("Couldn't get price of product");

                string imagePath = item.SelectSingleNode("./a/figure/img").GetAttributeValue("src", null);
                if (imagePath == null)
                {
                    info.WriteLog("Image Of product couldn't found");
                }
                string id = Path.GetFileName(url);


                Product p = new Product(this.WebsiteName, name, url, price, id, null);
                if(!Utils.SatisfiesCriteria(p, settings)) continue;

                if (settings.LoadImages) p.ImageUrl = imagePath;

                listOfProducts.Add(p);
            }

            info.State = Logger.ProcessingState.Success;
        }
    }
}
