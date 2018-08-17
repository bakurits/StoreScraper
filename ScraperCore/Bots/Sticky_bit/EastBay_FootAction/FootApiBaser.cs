using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper;
using StoreScraper.Attributes;
using StoreScraper.Bots.Bakurits.Antonioli;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
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

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private string GetIdFromUrl(string productUrl)
        {
            string suffix = productUrl.Substring(productUrl.LastIndexOf("/", StringComparison.Ordinal));
            return suffix.Substring(1, suffix.IndexOf(".", StringComparison.Ordinal) -1 );
        }

        private string GetPdpInfo(HtmlNodeCollection nodes)
        {
            foreach (var node in nodes)
            {
                if (node.InnerHtml.Contains("window.footlocker.pdpData"))
                {
                    return node.InnerHtml;
                }
            }

            throw new Exception("Couldn't get info");
        }

        private string GetInfoJson(HtmlNode document)
        {
            string preInfo = GetPdpInfo(document.SelectNodes("//script"));
            int startind = preInfo.IndexOf("=", StringComparison.Ordinal) + 1;
            int len = preInfo.LastIndexOf("}", StringComparison.Ordinal) - startind;
            return preInfo.Substring(startind, len+1);
        }

        private string GetImageUrlFromJson(JObject main)
        {
            JObject firstItem = JObject.Parse(main.GetValue("images").First.ToString());
            JObject biggestSizeObj = JObject.Parse(firstItem.GetValue("variations").Last.ToString());
            return biggestSizeObj.GetValue("url").ToString();
        }

        private string GetPriceFromJson(JObject main)
        {
            JObject sellableUnit = JObject.Parse(main.GetValue("sellableUnits").First.ToString());
            JObject price = JObject.Parse(sellableUnit.GetValue("price").ToString());
            return price.GetValue("formattedOriginalPrice").ToString();
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);

            string jsonStr = GetInfoJson(document);
            JObject mainObj = JObject.Parse(jsonStr);
            string id = GetIdFromUrl(productUrl);
            string url = productUrl;
            string img = GetImageUrlFromJson(mainObj);
            string name = mainObj.GetValue("name").ToString();
            Price productPrice = Utils.ParsePrice(GetPriceFromJson(mainObj));
            
            Product product = new Product(this, name, url, productPrice.Value, img, id, productPrice.Currency);
            Console.WriteLine(product);

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
