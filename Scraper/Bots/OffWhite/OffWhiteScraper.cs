using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Http;
using StoreScraper.Models;

namespace StoreScraper.Bots.OffWhite
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
                client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
                CollectCookies(client, token);
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
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="item"></param>
        /// <param name="info"></param>
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
        /// <param name="info"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            var url = "https://www.off---white.com" + item.SelectSingleNode("./a").GetAttributeValue("href", "");
            string name = item.SelectSingleNode("./a/figure/figcaption/div").InnerText;
            var priceNode = item.SelectSingleNode("./a/figure/figcaption/div[4]/span[1]/strong");
            bool parseSuccess = double.TryParse(priceNode.InnerText.Substring(2), NumberStyles.Any,
                CultureInfo.InvariantCulture, out var price);

            if (!parseSuccess) throw new WebException("Couldn't get price of product");

            string imagePath = item.SelectSingleNode("./a/figure/img").GetAttributeValue("src", null);
            if (imagePath == null)
            {
                Logger.Instance.WriteErrorLog("Image Of product couldn't found");
            }

            string id = item.GetAttributeValue("data-json-url", null);


            Product p = new Product(this, name, url, price, imagePath, id);
            if (!Utils.SatisfiesCriteria(p, settings)) return;

            p.ImageUrl = imagePath;

            listOfProducts.Add(p);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var jsonUrl = this.WebsiteBaseUrl + product.Id;
            ProductDetails result = new ProductDetails();

            HttpClient client = null;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    
                    if (_active)
                    {
                        client = CookieCollector.Default.GetClient();
                    }
                    else
                    {
                        client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true).AddHeaders(ClientFactory.FireFoxHeaders);
                        CollectCookies(client, token);
                    }
                    break;
                }
                catch
                {
                    //ingored
                }
            }

            
            HttpRequestMessage message = new HttpRequestMessage();
            message.Headers.Clear();
            message.Headers.TryAddWithoutValidation(ClientFactory.JsonXmlAcceptHeader.Key, ClientFactory.JsonXmlAcceptHeader.Value);
            message.Headers.TryAddWithoutValidation(ClientFactory.FirefoxUserAgentHeader.Key, ClientFactory.FirefoxUserAgentHeader.Value);

            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(jsonUrl);

            using (client)
            {
                var response = client.SendAsync(message).Result;
                response.EnsureSuccessStatusCode();
                var jsonStr = response.Content.ReadAsStringAsync().Result;
                JObject parsed = JObject.Parse(jsonStr);
                var sizes = parsed.SelectToken("available_sizes");
               
                foreach (JToken size in sizes.Children())
                {
                    var sizeName = (string) size.SelectToken("name");
                    result.SizesList.Add(sizeName);
                }
            }

            return result;
        }


        static void CollectCookies(HttpClient client, CancellationToken token)
        {

            var engine = new Jurassic.ScriptEngine();
            engine.SetGlobalValue("interop", "15");

            var task = client.GetAsync("https://www.off---white.com/en/US/", HttpCompletionOption.ResponseContentRead, token);
            var aa = task.Result.Content.ReadAsStringAsync().Result;

            HttpRequestMessage message = new HttpRequestMessage();
            message.Headers.Referrer = new Uri("https://www.off---white.com/en/US/");
            message.Headers.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri("https://www.off---white.com/favicon.ico");

            //client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.off---white.com/en/US/");
            //client.DefaultRequestHeaders.Remove("Accept");
            //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");

            var cc = client.SendAsync(message, token).Result;
            //client.DefaultRequestHeaders.Remove("Accept");
            //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", ClientFactory.ChromeAcceptHeader.Value);
            var pass = Regex.Match(aa, "name=\"pass\" value=\"(.*?)\"/>").Groups[1].Value;
            var answer = Regex.Match(aa, "name=\"jschl_vc\" value=\"(.*?)\"/>").Groups[1].Value;

            var script = Regex.Match(aa, "setTimeout\\(function\\(\\){(.*?)}, 4000\\);", RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups[1].Value;
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

            Task.Delay(5000, token).Wait();
            var message2 = new HttpRequestMessage();
            message2.Headers.Referrer = new Uri("https://www.off---white.com/en/US/");
            message2.RequestUri = new Uri($"https://www.off---white.com/cdn-cgi/l/chk_jschl?jschl_vc={answer}&pass={pass}&jschl_answer={calc}");
            message2.Method = HttpMethod.Get;

            var resultTask = client.SendAsync(message2, token);
            resultTask.Result.EnsureSuccessStatusCode();

        }
    }
}
