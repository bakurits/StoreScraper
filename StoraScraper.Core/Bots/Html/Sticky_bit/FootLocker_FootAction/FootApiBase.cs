using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Xml;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Sticky_bit.FootLocker_FootAction
{
   [DisableInGUI]
    public class FootAPIBase : ScraperBase
    {

        public override string WebsiteName { get; set; }
        public override string WebsiteBaseUrl { get; set; }
        public override bool Active { get; set; }
        public override Type SearchSettingsType { get; set; } = typeof(FootApiSearchSettings);
        private string ReleasePageApiEndpoint { get; set; }

        public FootAPIBase(string websiteName, string websiteBaseUrl)
        {
            WebsiteName = websiteName;
            WebsiteBaseUrl = websiteBaseUrl;
        }

  
        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            List <FootsitesProduct> products =  ScrapeReleasePage(token);
            products = products.Where((product) => product.ReleaseTime < DateTime.UtcNow).ToList();
            listOfProducts = products.Select((product) => (Product) product).ToList();
            Console.WriteLine(listOfProducts);
        }

        public List<FootsitesProduct> ScrapeReleasePage(CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient();
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri(ReleasePageApiEndpoint);
            var task = client.SendAsync(message).Result.Content.ReadAsStringAsync();
            task.Wait(token);

            if (task.IsFaulted) throw new JsonException("Can't get data");

            var productData = Utils.GetFirstJson(task.Result).GetValue("releases");
            var products = GetProducts(productData);
            return products;
        }

        private List<FootsitesProduct> GetProducts(JToken data)
        {
            var products = new List<FootsitesProduct>();
            foreach (var day in data)
            {
                var productsOnDay = day["products"];
                foreach (var productData in productsOnDay)
                    try
                    {
                        if (productData["availableInventory"].Value<int>() == 0) continue;

                        var date = GetDate(productData);
                        var name = GetPropertyAsString(productData, "name");
                        var price = GetPrice(productData);
                        var url = GetUrl(productData);
                        var image = GetPropertyAsString(productData, "primaryImageURL");
                        var sku = GetPropertyAsString(productData, "sku");
                        var model = GetPropertyAsString(productData, "model");
                        var color = GetPropertyAsString(productData, "color");
                        var showLaunchCountdown = GetCountDownEnabled(productData);
                        var gender = GetGender(productData);


                        var product = new FootsitesProduct(this, name, url, price, image, url, "USD", date)
                        {
                            Sku = sku,
                            Model = model,
                            Color = color,
                            Gender = gender,
                            LaunchCountdownEnabled = (showLaunchCountdown != 0)
                        };

                        products.Add(product);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
            }

            return products;
        }


        private double GetPrice(JToken productData)
        {
            if (productData["price"].Type != JTokenType.Null)
                return (double)productData["price"];
            return 0;
        }

        private string GetUrl(JToken productData)
        {
            if (productData["buyNowURL"].Type != JTokenType.Null)
            {
                string s = ((string)productData["buyNowURL"]).StringBefore("/?sid=");
                Uri uri = new Uri(s);
                string correctedUri = this.WebsiteBaseUrl + "/" + uri.PathAndQuery;
                return correctedUri;
            }
            return null;
        }

        private DateTime GetDate(JToken productData)
        {
            if (productData["launchDateTimeUTC"].Type == JTokenType.Null) return DateTime.MaxValue;
            var dateInJson = productData["launchDateTimeUTC"].Value<DateTime>();
            var date = dateInJson;
            return date;

        }

        private string GetPropertyAsString(JToken productData, string property)
        {
            if (productData[property].Type != JTokenType.Null)
                return (string)productData[property];
            return null;
        }

        private int GetCountDownEnabled(JToken productData)
        {
            if (productData["showLaunchCountdown"].Type != JTokenType.Null)
                return (int)productData["showLaunchCountdown"];
            return 1;
        }

        private string GetGender(JToken productData)
        {
            if (productData["availableSizes"].Type == JTokenType.Null) return "Not Available";
            if (((JArray)productData["availableSizes"])[0]["gender"].Type != JTokenType.Null)
                return (string)((JArray)productData["availableSizes"])[0]["gender"];
            return "Not Available";
        }


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {
            listOfProducts = new List<Product>();
            FootApiSearchSettings.GenderEnum gender;
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
            var client = ClientFactory.GetProxiedFirefoxClient();
            var message = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = new Uri(searchUrl)};
            message.Headers.Add("Accept", ClientFactory.JsonXmlAcceptHeader.Value);
            var responseText = client.SendAsync(message, token).Result.Content.ReadAsStringAsync().Result;
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(responseText);
            var products = xmlDocument.SelectNodes("productCategorySearchPage/products");
            if (products == null)
            {
                Logger.Instance.WriteVerboseLog("[Error] Unexpected XML!!");
                throw new WebException("Unexpected XML");
            }

            int sum = 0;

            foreach (XmlNode singleContact in products)
            {
                token.ThrowIfCancellationRequested();
                if (sum > 50) break;
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, singleContact, ref sum);
#else
                LoadSingleProductTryCatchWrapper(listOfProducts, settings, singleContact, ref sum);
#endif
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
        private void LoadSingleProductTryCatchWrapper(List<Product> listOfProducts, SearchSettingsBase settings,
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

        private void AddSizes(JObject main, ProductDetails productDetails, string subName)
        {
            JArray sellableUnit = JArray.Parse(main.GetValue("sellableUnits").ToString());
            foreach (var item in sellableUnit)
            {
                try
                {
                    string size = item["attributes"][0]["value"].ToString();
                    string description = item["attributes"][1]["value"].ToString();
                    string indicator = item["stockLevelStatus"].ToString();
                    if (indicator == "inStock" && description == subName)
                    {
                        productDetails.AddSize(size, description);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }
            }
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);

            string jsonStr = GetInfoJson(document);
            JObject mainObj = JObject.Parse(jsonStr);
            string id = GetIdFromUrl(productUrl);
            string url = productUrl;
            string img = GetImageUrlFromJson(mainObj);
            string subName = document.SelectSingleNode("//*[@class=\"label\"]").InnerText;
            Console.WriteLine(subName);
            string name = mainObj.GetValue("name").ToString();
            Price productPrice = Utils.ParsePrice(GetPriceFromJson(mainObj));
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

            AddSizes(mainObj, productDetails, subName);

            return productDetails;
        }

        public class FootLockerScraper : FootAPIBase
        {
            public FootLockerScraper() : base("FootLocker", "https://www.footlocker.com")
            {
                ReleasePageApiEndpoint = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/21";
            }

        }

        public class FootActionScraper : FootAPIBase
        {
            public FootActionScraper() : base("FootAction", "https://www.footaction.com")
            {
                ReleasePageApiEndpoint = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/34";
            }
        }
    }
}
