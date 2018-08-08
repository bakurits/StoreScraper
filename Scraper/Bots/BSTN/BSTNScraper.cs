using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using StoreScraper.Attributes;
using StoreScraper.Bots.ChampsSports_FootLocker_EastBay_FootAction;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.BSTN
{
    [DisabledScraper]
    public class BSTNScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }

        private string SearchPrefix = @"en/search/";
        private const string Keywords = @"{0}";
        private string SearchSuffix = @"/page/1/sort/date_new";

        private const string UlXpath = @"//*[@class=""block-grid four-up mobile-two-up productlist""]";


        public BSTNScraper()
        {
            this.WebsiteName = "BSTN";
            this.WebsiteBaseUrl = "https://www.bstn.com/";
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

            string searchUrl = WebsiteBaseUrl + SearchPrefix + string.Format(Keywords, settings.KeyWords) + SearchSuffix;

            HtmlNode container = null;
            HtmlNode node = InitialNavigation(searchUrl, token);
            container = node.SelectSingleNode(UlXpath);

            HtmlNodeCollection children = container.SelectNodes("./li/div");

            foreach (HtmlNode child in children)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, child);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, child);
#endif
            }

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

        private string ProcessPrice(string priceStr)
        {
            int i = 0;

            for (i = 0; i < priceStr.Length; i++)
            {
                if (!((priceStr[i] >= '0' && priceStr[i] <= '9') || priceStr[i] == '.'))
                {
                    break;
                }
            }

            return priceStr.Substring(0, i);
        }

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child)
        {
            string name = child.SelectSingleNode("./div[2]/a")?.GetAttributeValue("title", null);
            if (name == null) return;
            string link = WebsiteBaseUrl + child.SelectSingleNode("./div[1]/a").GetAttributeValue("href", null);
            string id = link.Substring(6);

            var priceNode = child.SelectSingleNode("./div[2]/a/span[1]");
            string salePriceStr = child.SelectSingleNode("/div[2]/a/span[2]")?.InnerText;

            string priceStr = (salePriceStr ?? priceNode.InnerText).Trim().Substring(1);
            string currency = Utils.GetCurrency(priceNode.InnerText);
            priceStr = ProcessPrice(priceStr);

            double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

            var imgUrl = child.SelectSingleNode("./div[1]/a/img")?.GetAttributeValue("src", null);

            Product product = new Product(this, name, link, price, id, imgUrl, currency);
            listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient();
            var node = client.GetDoc(product.Url, token)
                .DocumentNode;
            HtmlNodeCollection sizes = node.SelectNodes("//*[@class=\"product_sizes\"]//*[@class=\"button\"]");
            ProductDetails details = new ProductDetails();
            foreach (var s in sizes.Select(size => size.InnerText.Trim()))
            {
                details.AddSize(s, "Unknown");
            }

            return details;
        }
    }
}
