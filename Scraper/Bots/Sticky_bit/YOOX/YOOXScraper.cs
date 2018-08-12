using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            //Girls
            @"/girl/clothing/baby/shoponline?dept=clothinggirl_baby&sort=2&textsearch=",
            @"/girl/shoes/baby/shoponline?dept=shoesgirl_baby&sort=2&textsearch=",
            @"/girl/accessories/baby/shoponline?dept=accessoriesgirl_baby&sort=2&textsearch=",

            @"/girl/clothing/kids/shoponline?dept=collgirl_kid&sort=2&textsearch=",
            @"/girl/shoes/kids/shoponline?dept=shoesgirl_kid&sort=2&textsearch=",
            @"/girl/accessories/kids/shoponline?dept=accessoriesgirl_kid&sort=2&textsearch=",

            @"/girl/clothing/junior/shoponline?dept=clothinggirl_junior&sort=2&textsearch=",
            @"/girl/shoes/junior/shoponline?dept=shoesgirl_junior&sort=2&textsearch=",
            @"/girl/accessories/junior/shoponline?dept=accgirl_junior&sort=2&textsearch=",

            //Boys
            @"/boy/clothing/baby/shoponline?dept=collboy_baby&sort=2&textsearch=",
            @"/boy/clothing/baby/shoponline?dept=shoesboy_baby&sort=2&textsearch=",
            @"/boy/accessories/baby/shoponline?dept=accessoriesboy_baby&sort=2&textsearch=",

            @"/boy/clothing/kids/shoponline?dept=clothingboy_kid&sort=2&textsearch=",
            @"/boy/shoes/kids/shoponline?dept=shoesboy_kid&sort=2&textsearch=",
            @"/boy/accessories/kids/shoponline?dept=accessoriesboy_kid&sort=2&textsearch=",

            @"/boy/clothing/junior/shoponline?dept=clothingboy_junior&sort=2&textsearch=",
            @"/boy/shoes/junior/shoponline?dept=shoesboy_junior&sort=2&textsearch=",
            @"/boy/accessories/junior/shoponline?dept=accboy_junior&sort=2&textsearch=",

            //Women
            @"/women/shoponline?dept=women&textsearch=",

            //Men
            @"/men/shoponline?dept=men&textsearch="
        };

        private const string MainDivXpath = @"//*[@id=""itemsGrid""]/div[1]";


        public YOOXScraper()
        {
            this.WebsiteName = "YOOX";
            this.WebsiteBaseUrl = "https://www.yoox.com/us";
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

            foreach (var prefix in SearchPrefixes)
            {
                string searchUrl = WebsiteBaseUrl + prefix + settings.KeyWords;
                HtmlNode mainNode = InitialNavigation(searchUrl, token);
                HtmlNode mainDiv = mainNode.SelectSingleNode(MainDivXpath);

                HtmlNodeCollection childDivs = mainDiv.SelectNodes("./div");

                foreach (HtmlNode child in childDivs)
                {
                    string classValue = child.GetAttributeValue("class", null);
                    if (classValue == null || !classValue.Contains("col"))
                    {
                        continue;
                    }

#if DEBUG
                    LoadSingleProduct(listOfProducts, child);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, child);
#endif
                }
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

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child)
        {
            string id = child.SelectSingleNode("./div[1]")?.GetAttributeValue("data-current-cod10", null);
            string img = child.SelectSingleNode("./div[1]/div[1]/a[1]/img[1]")?.GetAttributeValue("rel", null);
            string url = child.SelectSingleNode("./div[1]/div[2]/a[1]")?.GetAttributeValue("href", null);
            string name = child.SelectSingleNode("./div[1]/div[2]/a[1]/div[1]").InnerText + " (" + id + ")";
            HtmlNodeCollection priceSpans = child.SelectNodes("./div[1]/div[2]/a/div[3]/span");
            if (priceSpans == null)
            {
                return;
            }
            string priceStr = priceSpans[priceSpans.Count - 1].InnerHtml;

            List<string> sizes;
            Price price;
            try
            {
                sizes = getSizeArray(child.SelectSingleNode("./div[1]/div[2]/a[1]/div[4]/div[1]"));
                price = Utils.ParsePrice(priceStr); ;
            }
            catch (Exception e)
            {
                return;
            }

            listOfProducts.Add(new Product(this, name, WebsiteBaseUrl + url.Substring(3), price.Value, img, id, price.Currency));

        }

        private List<string> getSizeArray(HtmlNode sizeContainer)
        {
            List<string> sizes = new List<string>();
            HtmlNodeCollection children = sizeContainer.SelectNodes("./span");

            foreach (var child in children)
            {
                sizes.Add(child.InnerText);
            }

            return sizes;
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            return null;
        }
    }
}
