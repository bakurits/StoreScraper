using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using System.Net.Http;
using System.Threading.Tasks;
using StoreScraper.Attributes;
using StoreScraper.Http;

namespace StoreScraper.Bots.DavitBezhanishvili.SneakerStudioScraper
{
    public class SneakerStudioScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "SneakerStudio";
        public override string WebsiteBaseUrl { get; set; } = "https://sneakerstudio.com";


        private bool _active;
        public override bool Active
        {
            get => _active;
            set
            {
                if (value)
                {

                    Task.WaitAll
                    (
                        Task.Run(() =>
                        {
                            Parallel.ForEach(ClientFactory.Storage.Value.ProxiedClients.Values, client =>
                            {
                                try
                                {
                                    client.GetAsync(NewArrivalsUrl).Result.EnsureSuccessStatusCode();
                                    HttpRequestMessage message = new HttpRequestMessage();
                                    message.Method = HttpMethod.Get;
                                    message.RequestUri = SettingsUrl;
                                    message.Headers.Referrer = NewArrivalsUrl;
                                    client.SendAsync(message).Result.EnsureSuccessStatusCode();
                                }
                                catch
                                {
                                    //ignored
                                }
                            });
                        }),

                        Task.Run(() =>
                        {
                            try
                            {
                                HttpRequestMessage message = new HttpRequestMessage();
                                message.Method = HttpMethod.Get;
                                message.RequestUri = SettingsUrl;
                                message.Headers.Referrer = NewArrivalsUrl;
                                ClientFactory.Storage.Value.ProxilessClient.SendAsync(message).Result.EnsureSuccessStatusCode();
                            }
                            catch
                            {
                                //ignored
                            }
                        })

                    );
                    _active = true;
                }
                else _active = false;
            }
        }


        private readonly Uri NewArrivalsUrl = new Uri("https://sneakerstudio.com/New-snewproducts-eng.html?newproducts=y&");
        private readonly Uri SettingsUrl = new Uri("https://sneakerstudio.com/settings.php?sort_order=date-d&curr=USD");
        private readonly Uri SettingsUrl2 = new Uri(@"https://sneakerstudio.com/settings.php?curr=USD");
      

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var client = ClientFactory.GetProxiedFirefoxClient();

            HtmlDocument doc = null;

            if (!Active)
            {
                HttpRequestMessage message = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = SettingsUrl
                };
                message.Headers.Referrer = NewArrivalsUrl;
                doc = client.GetDoc(message, token);
            }
            else
            {
                doc = client.GetDoc(NewArrivalsUrl.AbsoluteUri, token);
            }



            Scrap(doc, ref listOfProducts, null, token);
            FillProductNames(ref listOfProducts, token);
        }


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();

            var referer = new Uri($"https://sneakerstudio.com/search.php?text={settings.KeyWords}");

            var client = ClientFactory.GetProxiedFirefoxClient();
            HtmlDocument doc = null;

            if (!Active)
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.Method = HttpMethod.Get;
                message.RequestUri = SettingsUrl2; ///////////////// change to settingsUrl
                message.Headers.Referrer = referer;
                doc = client.GetDoc(message, token);
            }
            else
            {
                doc = client.GetDoc(referer.AbsoluteUri, token);
            }

            Scrap(doc, ref listOfProducts, settings, token);
        }

        private void Scrap(HtmlDocument document, ref List<Product> listOfProducts, SearchSettingsBase settings,
            CancellationToken token)
        {

            var nodes = document.DocumentNode.SelectSingleNode("//div[@class = 'row']");
            if (nodes == null)
            {
                Logger.Instance.WriteErrorLog("Unexcepted Html");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Unexcepted Html");
            }

            var children = nodes.SelectNodes("./div[@class = 'product_wrapper col-md-4 col-xs-6']");
            if (children == null)
            {
                return;
            }

            foreach (var child in children)
                if (child.SelectSingleNode("./a[@class = 'product-icon']/div[@class ='comingsoon']") == null)
                {
                    token.ThrowIfCancellationRequested();
#if DEBUG
                    LoadSingleProduct(listOfProducts, child, settings);
#else
                    LoadSingleProductTryCatchWraper(listOfProducts, child, settings);
#endif
                }
        }


        private void FillProductNames(ref List<Product> listOfProducts, CancellationToken token)
        {
            CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            listOfProducts = listOfProducts.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(500).Select
            (
                prod =>
                {
                    try
                    {

                        if (!string.IsNullOrWhiteSpace(prod.Name)) return prod;
                        var product = (Product)GetProductDetails(prod.Url, tokenSource.Token);
                        tokenSource.CancelAfter(1000);
                        return product;
                    }
                    catch
                    {
                        return prod;
                    }
                }
            ).ToList();
        }

        private Product FillProduct(Product p, CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(p.Name)) return p;
            try
            {
                p = GetProductDetails(p.Url, token);
            }
            catch
            {
                //ignored
            }

            return p;
        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="child"></param>
        /// <param name="settings"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            try
            {
                LoadSingleProduct(listOfProducts, child, settings);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }

        private string GetImageUrl(HtmlNode child)
        {
            var picNode = child.SelectSingleNode("./a[@class = 'product-icon']");
            return WebsiteBaseUrl + picNode.SelectSingleNode("./img").GetAttributeValue("data-src", null);
        }

        private string GetProductUrl(HtmlNode child)
        {
            return WebsiteBaseUrl + child.SelectSingleNode("./a[@class = 'product-icon']").GetAttributeValue("href", null);
        }


        private string GetProductName(HtmlNode child)
        {
            return child.SelectSingleNode("./h3/a").InnerText;

        }


        private void LoadSingleProduct(List<Product> listOfProducts, HtmlNode child, SearchSettingsBase settings)
        {
            Price p = GetProductPrice(child);
            var imageUrl = GetImageUrl(child);
            var productUrl = GetProductUrl(child);
            var productName = GetProductName(child);

            var product = new Product(this, productName, productUrl, p.Value, imageUrl, productUrl, p.Currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private Price GetProductPrice(HtmlNode child)
        {
            var priceNode = child.SelectSingleNode("./div[@class = 'product_prices']/span[@class = 'price']");
            string priceStr = priceNode.SelectSingleNode("./text()").InnerText;
            Price p = Utils.ParsePrice(priceStr);
            return p;
        }




        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient();

            HtmlDocument doc = null;

            if (!Active)
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.Method = HttpMethod.Get;
                message.RequestUri = SettingsUrl2;
                message.Headers.Referrer = new Uri(productUrl);
                doc = client.GetDoc(message, token);
            }
            else
            {
                doc = client.GetDoc(productUrl, token);
            }

            var webPage = doc.DocumentNode;

            ProductDetails details = ConstructProduct(webPage, productUrl);

            var jsonStr = GetJson(webPage.InnerHtml);
            if (jsonStr == "") return details;

            JObject parsed = JObject.Parse(jsonStr);
            var sizes = parsed.SelectTokens("sizes");
            foreach (JToken szToken in sizes.Children())
            {
                var sz = szToken.First;
                var size = (string)sz.SelectToken("name");
                var amount = (string)sz.SelectToken("amount");
                if (amount != "0")
                    details.AddSize(size, amount);
            }
            return details;
        }

        private string GetJson(string webPageInnerHtml)
        {
            string json = "{";
            var str = "var product_data = {";
            var ind = webPageInnerHtml.IndexOf(str, StringComparison.Ordinal);
            ind += str.Length;
            int parCount = 1;
            while (parCount > 0)
            {
                json += webPageInnerHtml[ind];
                if (webPageInnerHtml[ind] == '}')
                    parCount--;
                if (webPageInnerHtml[ind] == '{')
                    parCount++;
                ind++;
            }

            return json;
        }

        private ProductDetails ConstructProduct(HtmlNode webPage, string productUrl)
        {
            var name = webPage.SelectSingleNode("//div[@class='projector_navigation']/h1").InnerText.Trim();

            var picNode = webPage.SelectSingleNode(
                "//div[contains(@class,'photos col-md-7 col-xs-12')]/a[@id ='projector_image_1']/img");
            string image = null;
            if (picNode != null)
                image = WebsiteBaseUrl + picNode.GetAttributeValue("src", null);
            var priceNode = webPage.SelectSingleNode("//strong[@class='projector_price_value']");
            var txt = priceNode.InnerText.Trim();
            var price = Utils.ParsePrice(txt);
            var keyWords = webPage.SelectSingleNode("//meta[@name = 'keywords']").GetAttributeValue("content", null);

            ProductDetails details = new ProductDetails()
            {
                StoreName = WebsiteName,
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                KeyWords = keyWords,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };
            return details;
        }


    }
}
