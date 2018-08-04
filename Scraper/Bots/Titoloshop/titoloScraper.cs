using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.titoloshop
{
    class TitoloScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Titoloshop";
        public override string WebsiteBaseUrl { get; set; } = "https://en.titoloshop.com";
        public override bool Active { get; set; }
        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var searchUrl =
                $"https://en.titoloshop.com/catalogsearch/result/index/?dir=desc&order=created_at&q={settings.KeyWords}";
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(searchUrl, token);
            Logger.Instance.WriteErrorLog("Unexpected html!");
            var nodes = document.DocumentNode.SelectSingleNode("//span[contains(@class, 'no-bullet') and contains(@class, 'small-block-grid-2')]");
            var children = nodes.SelectNodes("./li");
            if (children == null)
            {
                Logger.Instance.WriteErrorLog("Unexpected html");
                throw new HtmlWebException("Unexpected html");
            }

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child);
#else
                LoadSingleProductTryCatchWraper(listOfProducts,child);
#endif
            }
            throw new NotImplementedException();
        }


        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="info"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child)
        {
            string imageURL = child.SelectSingleNode(".//a[contains(@class, 'product-image')]/img").GetAttributeValue("src", null);
            string productName = child.SelectSingleNode(".//span[contains(@class,'name')]").InnerText;
            string productURL = child.SelectSingleNode(".//a[contains(@class,'long-productname')]").GetAttributeValue("href",null);
            string priceIntoString = child.SelectSingleNode(".//span[contains(@class,'price')]").InnerText;
            double price = getPrice(priceIntoString);
            
        }

        private double getPrice(string priceIntoString)
        {
            string result = Regex.Replace(priceIntoString, @"[^\d]", "");
            return Convert.ToDouble(result);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
