using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Http;
using StoreScraper.Models;

namespace StoreScraper.Bots.GiorgiChkhikvadze
{
    [Serializable]
    public class OffWhiteScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; } = "Off---white";
        public sealed override string WebsiteBaseUrl { get; set; } = "https://Off---white.com";

        private bool _active;

        public override bool Active
        {
            get => _active;
            set
            {
                if (!_active && value)
                {
                    CookieCollector.Default.RegisterActionAsync(this.WebsiteName, CollectCookies, TimeSpan.FromMinutes(20)).Wait();
                    _active = true;
                }
                else if(_active && !value)
                {
                    CookieCollector.Default.RemoveAction(this.WebsiteName);
                    _active = false;
                }
            }
        }

        [Browsable(false)]
        public List<string> CurrentCart { get; set; } = new List<string>();

        private const string SearchUrlFormat = @"https://www.off---white.com/en/US/search?q={0}";


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings
            , CancellationToken token)
        {

            listOfProducts = new List<Product>();

            var searchUrl = string.Format(SearchUrlFormat, settings.KeyWords);

            HttpClient client = null;


            if (_active)
            {
                client = CookieCollector.Default.GetClient();
            }
            else
            {
                for (int i = 0; i < AppSettings.Default.ProxyRotationRetryCount; i++)
                {
                    try
                    {
                        client = ClientFactory.GetProxiedFirefoxClient();
                        CollectCookies(client, token);
                        break;
                    }
                    catch
                    {
                        if (i != AppSettings.Default.ProxyRotationRetryCount - 1) continue;
                        Logger.Instance.WriteErrorLog($"Can't Connect to off---white");
                        throw new WebException("Can't connect to website");
                    }

                }
            }

            var document = client.GetDoc(searchUrl, token);


            if (document == null)
            {
                Logger.Instance.WriteErrorLog($"Can't Connect to off---white");
                throw new WebException("Can't connect to website");
            }

            var node = document.DocumentNode;
            var container = node.SelectSingleNode("//section[@class='products']");

            if (container == null)
            {
                Logger.Instance.WriteErrorLog("Uncexpected Html!!");
                Logger.Instance.SaveHtmlSnapshop(document);
                throw new WebException("Undexpected Html");
            }

            var items = container.SelectChildren("article");



            foreach (var item in items)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, item);
#endif
            }
        }


        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, item);
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
        /// <param name="settings"></param>
        /// <param name="item"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            var url = "https://www.off---white.com" + item.SelectSingleNode("./a").GetAttributeValue("href", "");
            string name = item.SelectSingleNode("./a/figure/figcaption/div").InnerText;
            var priceNode = item.SelectSingleNode("./a/figure/figcaption/div[4]/span[1]/strong");

            var price = Utils.ParsePrice(priceNode.InnerText);

            string imagePath = item.SelectSingleNode("./a/figure/img").GetAttributeValue("src", null);
            if (imagePath == null)
            {
                Logger.Instance.WriteErrorLog("Image Of product couldn't found");
            }

            string id = item.GetAttributeValue("data-json-url", null);


            Product p = new Product(this, name, url, price.Value, imagePath, id, price.Currency);
            if (!Utils.SatisfiesCriteria(p, settings)) return;

            p.ImageUrl = imagePath;

            listOfProducts.Add(p);
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            HttpClient client = null;

            for (int i = 0; i < AppSettings.Default.ProxyRotationRetryCount; i++)
            {
                try
                {
                    
                    if (_active)
                    {
                        client = CookieCollector.Default.GetClient();
                    }
                    else
                    {
                        client = ClientFactory.GetProxiedFirefoxClient();
                        CollectCookies(client, token);
                    }
                    break;
                }
                catch
                {
                    if (i != AppSettings.Default.ProxyRotationRetryCount - 1) continue;
                    Logger.Instance.WriteErrorLog("Cookie collecting failed");
                    throw new Exception("Cookie collecting failed");
                }
            }

            var doc = client.GetDoc(productUrl, token);
            var root = doc.DocumentNode;
            var sizeNodes = root.SelectNodes("//*[contains(@class,'styled-radio')]/label");
            var sizes = sizeNodes.Select(node => node.InnerText).ToList();

            var name = root.SelectSingleNode("//*[contains(@class, 'prod-title')]").InnerText.Trim();
            var priceNode = root.SelectSingleNode("//div[contains(@class, 'price')]/span/strong");
            var price = Utils.ParsePrice(priceNode.InnerText);
            var image = root.SelectSingleNode("//*[@id='image-0']").GetAttributeValue("src", null);

            ProductDetails result = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };

            foreach (var size in sizes)
            {
                result.AddSize(size, "Unknown");
            }

            return result;
        }


        static void CollectCookies(HttpClient client, CancellationToken token)
        {

            var engine = new Jurassic.ScriptEngine();
            engine.SetGlobalValue("interop", "15");

            var task = client.GetAsync("https://www.off---white.com/en/US/", HttpCompletionOption.ResponseContentRead, token);

            using (var result = task.Result)
            {
                if (result.IsSuccessStatusCode) return;

                var aa = result.Content.ReadAsStringAsync().Result;

                using (var message = new HttpRequestMessage())
                {
                    message.Headers.Referrer = new Uri("https://www.off---white.com/en/US/");
                    message.Headers.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
                    message.Method = HttpMethod.Get;
                    message.RequestUri = new Uri("https://www.off---white.com/favicon.ico");

                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.off---white.com/en/US/");
                    //client.DefaultRequestHeaders.Remove("Accept");
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");

                    var cc = client.SendAsync(message, token).Result;
                    cc.Dispose();
                }

               
                //client.DefaultRequestHeaders.Remove("Accept");
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", ClientFactory.ChromeAcceptHeader.Value);
                var pass = Regex.Match(aa, "name=\"pass\" value=\"(.*?)\"/>").Groups[1].Value;
                var answer = Regex.Match(aa, "name=\"jschl_vc\" value=\"(.*?)\"/>").Groups[1].Value;

                var script = Regex.Match(aa, "setTimeout\\(function\\(\\){(.*?)}, 4000\\);",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups[1].Value;
                script = script.Replace("a = document.getElementById('jschl-answer');", string.Empty);
                script = script.Replace("f.action += location.hash;", string.Empty);
                script = script.Replace("f.submit();", string.Empty);
                script = script.Replace("f = document.getElementById('challenge-form');", string.Empty);
                script = script.Replace("a.value", "interop");
                script = script.Replace("t = document.createElement('div');", string.Empty);
                script = script.Replace("t.innerHTML=\"<a href='/'>x</a>\";", string.Empty);
                script = script.Replace("t = t.firstChild.href", "t='https://www.off---white.com/';");



                var gga = engine.Evaluate(script);
                var calc = engine.GetGlobalValue<string>("interop");

                Task.Delay(5000, token).Wait(token);
                using (var message2 = new HttpRequestMessage())
                {
                
                    message2.Headers.Referrer = new Uri("https://www.off---white.com/en/US/");
                    message2.RequestUri =
                        new Uri(
                            $"https://www.off---white.com/cdn-cgi/l/chk_jschl?jschl_vc={answer}&pass={pass}&jschl_answer={calc}");
                    message2.Method = HttpMethod.Get;

                    using (var resultTask = client.SendAsync(message2, token))
                    {
                        resultTask.Result.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}
