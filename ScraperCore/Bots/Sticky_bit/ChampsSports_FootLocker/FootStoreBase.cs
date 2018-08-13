using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Xml;
using HtmlAgilityPack;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker
{
    public class Gender
    {
        public string name { get; set; }
        public string id { get; set; }
        public string cmRef { get; set; }
        public string crumbs { get; set; }
    }

    [DisabledScraper]
    public class FootSimpleBase : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }

        private string UrlPrefix;
        private const string PageSizeSuffix = @"?Ns=P_NewArrivalDateEpoch%7C1&cm_SORT=New%20Arrivals";
        private const string Keywords = @"/keyword-{0}";
        private const string GenderPrefix = @"/{0}/_-_/N-{1}";
        private const string GenderPostfix = @"&crumbs={0}&cm_REF={1}";
        private const string UlXpath = @"//*[@id=""endeca_search_results""]/ul";

        Dictionary<int, Gender> genders;


        public FootSimpleBase(string websiteName, string websiteBaseUrl)
        {
            this.WebsiteName = websiteName;
            this.WebsiteBaseUrl = websiteBaseUrl;
            this.UrlPrefix = websiteBaseUrl + "/_-_";

            if (websiteName == "ChampsSports")
            {
                genders = new Dictionary<int, Gender>()
                {
                    { 1, new Gender {name="Mens", id="24",crumbs="76", cmRef="Men%27s"}},
                    { 2, new Gender {name="Womens", id="25",crumbs="77", cmRef="Women%27s"}},
                    { 3, new Gender {name="Boys", id="22",crumbs="2689", cmRef="Boys%27"}},
                    { 4, new Gender {name="Girls", id="25",crumbs="2690", cmRef="Girls%27"}},
                };
            }
            else if (websiteName == "EastBay")
            {
                genders = new Dictionary<int, Gender>()
                {
                    { 1, new Gender {name="Mens", id="1p",crumbs="61", cmRef="Men%27s"}},
                    { 2, new Gender {name="Womens", id="1q",crumbs="62", cmRef="Women%27s"}},
                    { 3, new Gender {name="Boys", id="1pi",crumbs="2214", cmRef="Boys%27"}},
                    { 4, new Gender {name="Girls", id="1pj",crumbs="2215", cmRef="Girls%27"}},
                };
            }
        }

        private HtmlNode InitialNavigation(string url, CancellationToken token)
        {
            HttpClient ClientGenerator() =>
                ClientFactory.GetProxiedFirefoxClient();
            var document = Utils.GetDoc(ClientGenerator, url, 4, 5, token);
            return document.DocumentNode;
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();

            string searchUrl = UrlPrefix + string.Format(Keywords, settings.KeyWords) + PageSizeSuffix;

            int gender = 1; // 0 - all, 1 - mens, 2 - womens, 3 - boys, 4 - girls 
            if (gender != 0)
            {
                searchUrl = WebsiteBaseUrl + string.Format(GenderPrefix, genders[gender].name, genders[gender].id) + string.Format(Keywords, settings.KeyWords) + PageSizeSuffix + string.Format(GenderPostfix, genders[gender].crumbs, genders[gender].cmRef);
            }

            HtmlNode container = null;

            while (container == null)
            {
                HtmlNode node = InitialNavigation(searchUrl, token);
                container = node.SelectSingleNode(UlXpath);
            }

            HtmlNodeCollection children = container.SelectNodes("./li");

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

        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child)
        {
            string id = child.GetAttributeValue("data-sku", null);
            string name = child.SelectSingleNode(".//*[contains(@class, 'product_title')]")?.InnerText;
            if (name == null) return;
            string link = child.SelectSingleNode("./a").GetAttributeValue("href", null);

            var priceNode = child.SelectSingleNode(".//*[contains(@class, 'product_price')]");
            string salePriceStr = priceNode.SelectSingleNode("./*[contains(@class, 'sale')]")?.InnerText;

            string priceStr = (salePriceStr ?? priceNode.InnerText).Trim().Substring(1);
            int i = 0;

            for (i = 0; i < priceStr.Length; i++)
            {
                if (!((priceStr[i] >= '0' && priceStr[i] <= '9') || priceStr[i] == '.'))
                {
                    break;
                }
            }

            priceStr = priceStr.Substring(0, i);
            double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

            var imgUrl = child.SelectSingleNode("./a/img")?.GetAttributeValue("src", null);
            imgUrl = imgUrl ?? child.SelectSingleNode("./a/span/img").GetAttributeValue("data-original", null);

            Product product = new Product(this, name, link, price, id, imgUrl);
            listOfProducts.Add(product);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient().AddHeaders(ClientFactory.HtmlOnlyHeader);
            var node = client.GetDoc(productUrl, token)
                .DocumentNode;
            HtmlNodeCollection sizes = node.SelectNodes("//*[@class=\"product_sizes\"]//*[@class=\"button\"]");
            ProductDetails details = new ProductDetails();
            foreach (var s in sizes.Select(size => size.InnerText.Trim()))
            {
                details.AddSize(s, "Unknown");
            }

            return details;
        }

        public class ChampsSportsScraper : FootSimpleBase
        {
            public ChampsSportsScraper() : base("ChampsSports", "https://www.champssports.com")
            {
            }
        }

        public class EastBayScraper : FootSimpleBase
        {
            public EastBayScraper() : base("EastBay", "https://www.eastbay.com")
            {
            }
        }
    }


}