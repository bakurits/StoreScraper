using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Sticky_bit.YOOX
{
    [DisabledScraper]
    public class YOOXScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }

        private string[] SearchPrefixes = {
                @"girl/clothing/baby/shoponline?dept=clothinggirl_baby&sort=2&textsearch="
                
        };
        private const string Keywords = @"{0}";
        private string SearchSuffix = @"/page/1/sort/date_new";

        private const string UlXpath = @"//*[@class=""block-grid four-up mobile-two-up productlist""]";


        public YOOXScraper()
        {
            this.WebsiteName = "YOOX";
            this.WebsiteBaseUrl = "https://www.yoox.com/us/";
            this.Active = true;
        }

        private HtmlNode InitialNavigation(string url, CancellationToken token)
        {
            var request = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = request.GetDoc(url, token);

            return document.DocumentNode;
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
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

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child)
        {
            
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            return null;
        }
    }
}
