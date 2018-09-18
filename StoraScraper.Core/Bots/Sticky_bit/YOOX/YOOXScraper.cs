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
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Sticky_bit.YOOX
{
    public class YOOXScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }

        private string[] SearchPrefixes = {
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
            List <Product> localList = new List<Product>();

            SearchPrefixes.AsParallel().ForAll(prefix =>
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
                    LoadSingleProduct(localList, child);
#else
                LoadSingleProductTryCatchWraper(localList, child);
#endif
                }
            });

            listOfProducts = localList;
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
            catch
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

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private static string getID(string url)
        {
            return url.Substring(url.IndexOf("us/")+3, 10);
        }

        private void addSizes(HtmlNode document, ProductDetails productDetails)
        {
            HtmlNodeCollection liNodes = document.SelectNodes("//*[@id=\"itemSizes\"]/ul/li");
            foreach (var liNode in liNodes)
            {
                productDetails.AddSize(Utils.EscapeNewLines(liNode.InnerText), "Unknown");
            }
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            string id = getID (productUrl);
            string url = productUrl;
            string img = document.SelectSingleNode("//*[@class=\"item-image\"]/img")?.GetAttributeValue("src", null);
            string name = Utils.EscapeNewLines(document.SelectSingleNode("//*[@id=\"itemTitle\"]/h1/a").InnerText);
            Price productPrice = Utils.ParsePrice(document.SelectSingleNode("//*[@itemprop=\"price\"]")?.InnerText);
            ProductDetails productDetails = new ProductDetails()
            {
                Name = name,
                Price = productPrice.Value,
                ImageUrl = img,
                Url = productUrl,
                Id = productUrl,
                Currency = productPrice.Currency,
                ScrapedBy = this
            };

            addSizes(document, productDetails);
            return productDetails;
        }
    }
}
