using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Sticky_bit.ChampsSports_EastBay
{
    public class Gender
    {
        public string name { get; set; }
        public string id { get; set; }
        public string cmRef { get; set; }
        public string crumbs { get; set; }
    }

    public class Color
    {
        public string name { get; set; }
        public string id { get; set; }
        public string cmRef { get; set; }
        public string crumbs { get; set; }

    }

    [DisableInGUI]
    public class FootSimpleBase : ScraperBase
    {
        public sealed override string WebsiteName { get; set; }
        public sealed override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }

        private string UrlPrefix;
        private const string PageSizeSuffix = @"?Ns=P_NewArrivalDateEpoch%7C1&cm_SORT=New%20Arrivals";
        private const string Keywords = @"/keyword-{0}";
        private const string GenderColorPrefix = @"{0}/_-_/N-{1}";
        private const string GenderPostfix = @"&crumbs={0}&cm_REF={1}";
        private const string UlXpath = @"//*[@id=""endeca_search_results""]/ul";

        Dictionary<int, Gender> genders;
        Dictionary<int, Color> colors;


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

                colors = new Dictionary<int, Color>()
                {
                    { 1, new Color {name="Pink", id="dx",crumbs="501", cmRef="Pink"}},
                    { 2, new Color {name="Red", id="di",crumbs="486", cmRef="Red"}},
                    { 3, new Color {name="Orange", id="do",crumbs="492", cmRef="Orange"}},
                    { 4, new Color {name="Yellow", id="dy",crumbs="502", cmRef="Yellow"}},
                    { 5, new Color {name="Green", id="dg",crumbs="484", cmRef="Green"}},
                    { 6, new Color {name="Blue", id="df",crumbs="483", cmRef="Blue"}},
                    { 7, new Color {name="Pruple", id="dj",crumbs="487", cmRef="Purple"}},
                    { 8, new Color {name="Black", id="de",crumbs="482", cmRef="Black"}},
                    { 9, new Color {name="Grey", id="ds",crumbs="496", cmRef="Grey"}},
                    { 10, new Color {name="White", id="dl",crumbs="489", cmRef="White"}},
                    { 11, new Color {name="Tan", id="du",crumbs="498", cmRef="Tan"}},
                    { 12, new Color {name="Brown", id="dv",crumbs="499", cmRef="Brown"}},
                    { 13, new Color {name="Multicolor", id="e3",crumbs="507", cmRef="Multicolor"}},

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


                colors = new Dictionary<int, Color>()
                {
                    { 1, new Color {name="Pink", id="ar",crumbs="387", cmRef="Pink"}},
                    { 2, new Color {name="Red", id="a9",crumbs="369", cmRef="Red"}},
                    { 3, new Color {name="Orange", id="ah",crumbs="377", cmRef="Orange"}},
                    { 4, new Color {name="Yellow", id="aj",crumbs="379", cmRef="Yellow"}},
                    { 5, new Color {name="Green", id="a7",crumbs="367", cmRef="Green"}},
                    { 6, new Color {name="Blue", id="a6",crumbs="366", cmRef="Blue"}},
                    { 7, new Color {name="Pruple", id="aa",crumbs="370", cmRef="Purple"}},
                    { 8, new Color {name="Black", id="a5",crumbs="365", cmRef="Black"}},
                    { 9, new Color {name="Grey", id="ad",crumbs="373", cmRef="Grey"}},
                    { 10, new Color {name="White", id="ab",crumbs="371", cmRef="White"}},
                    { 11, new Color {name="Tan", id="an",crumbs="383", cmRef="Tan"}},
                    { 12, new Color {name="Brown", id="ao",crumbs="384", cmRef="Brown"}},
                    { 13, new Color {name="Multicolor", id="at",crumbs="389", cmRef="Multicolor"}},

                };
            }
        }

        private HtmlNode InitialNavigation(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient();
            var document = client.GetDoc(url, token);
            return document.DocumentNode;
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();

            string searchUrl = UrlPrefix + string.Format(Keywords, settings.KeyWords) + PageSizeSuffix;

            //int genderId = 2; // 0 - all, 1 - mens, 2 - womens, 3 - boys, 4 - girls 
            //int colorId = 9;
            int genderId = 0;
            int colorId = 0;

            if (genderId != 0 || colorId != 0)
            {
                Color color = null;
                Gender gender = null;

                string genderString = "";
                string crumbs = "";
                string cmREF;
                if (genderId != 0)
                {
                    gender = genders[genderId];
                    genderString = "/" + gender.name;
                }

                if (colorId != 0)
                {
                    color = colors[colorId];
                }


                string colorGenderId = "";

                if (color != null && gender != null)
                {
                    colorGenderId = gender.id + "Z" + color.id;
                    crumbs = gender.crumbs + " " + color.crumbs;
                    cmREF = color.cmRef;
                }
                else if (color != null)
                {
                    colorGenderId = color.id;
                    crumbs = color.crumbs;
                    cmREF = color.cmRef;

                }
                else
                {
                    colorGenderId = gender.id;
                    crumbs = gender.crumbs;
                    cmREF = gender.cmRef;

                }


                searchUrl = WebsiteBaseUrl + string.Format(GenderColorPrefix, genderString, colorGenderId) + string.Format(Keywords, settings.KeyWords) + PageSizeSuffix + string.Format(GenderPostfix, crumbs, cmREF);
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

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            return client.GetDoc(url, token).DocumentNode;

        }

        private JObject GetMainJSON(HtmlNode document)
        {
            HtmlNodeCollection scriptNodes = document.SelectNodes("//script");
            Regex modelRegex = new Regex("var model *\t*\n*\r*= *{ *\t*\n*\r*");
            
            foreach (var scriptNode in scriptNodes)
            {
                string script = scriptNode.InnerHtml;
                if (modelRegex.IsMatch(script))
                {
                    Match firstMatch = modelRegex.Match(script);
                    string afterModel = script.Substring(firstMatch.Index);
                    string afterBracket = afterModel.Substring(afterModel.IndexOf("{", StringComparison.Ordinal));
                    string betweenModels = afterBracket.Substring(0, afterBracket.IndexOf("var model", StringComparison.Ordinal));
                    string JSONString = betweenModels.Substring(0, betweenModels.LastIndexOf("}", StringComparison.Ordinal)+1);
                    return JObject.Parse(JSONString);
                }
            }
            return new JObject();
        }

        private string getImg(HtmlNode document)
        {
            string img = document.SelectSingleNode("//*[@id=\"product_mixedmedia\"]//img")?.GetAttributeValue("src", null);
            if (img == null)
            {
                img = document.SelectSingleNode("//*[@id=\"product_images\"]//img")?.GetAttributeValue("src", null);
            }
            int ind = 0;
            while (img[ind] == '/')
            {
                ind++;
            }

            img = img.Substring(ind);

            return img;
        }

        private void addSizes(JObject main, ProductDetails productDetails)
        {
            JArray sizeArr = JArray.Parse(main["AVAILABLE_SIZES"].ToString());
            foreach (var elem in sizeArr)
            {
                productDetails.AddSize(elem.ToString(), "Unknown");
            }
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            JObject mainObj = GetMainJSON(document);

            string id = mainObj["ALLSKUS"].First.ToString();
            string img = getImg(document);
            Price price;
            if (mainObj["PR_SALE"] != null)
            {
                price = Utils.ParsePrice(mainObj["PR_SALE"].ToString());
            }
            else
            {
                price = Utils.ParsePrice(mainObj["PR_LIST"].ToString());
            }

            string name = mainObj["NM"].ToString();

            ProductDetails productDetails = new ProductDetails()
            {
                Name = name,
                Price = price.Value,
                ImageUrl = img,
                Url = productUrl,
                Id = id,
                Currency = "USD",
                ScrapedBy = this
            };

            addSizes(mainObj, productDetails);

            return productDetails;
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