using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Browser;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.OffWhite
{
    [Serializable]
    public class OffWhiteScraper : ScraperBase
    {
        public sealed override string WebsiteName { get; set; } = "Off---white";
        public sealed override string WebsiteBaseUrl { get; set; } = "Off---white.com";

        private bool _active;

        public override bool Active
        {
            get => _active;
            set
            {
                _active = value;
                if (value)
                {
                    CookieCollector.Default.RegisterAction(this.WebsiteName, CollectCookies, TimeSpan.FromMinutes(5));
                }
                else
                {
                    CookieCollector.Default.RemoveAction(this.WebsiteName);
                }
            }
        }

        [Browsable(false)]
        public List<string> CurrentCart { get; set; } = new List<string>();

        private const string SearchUrlFormat = @"https://www.off---white.com/en/US/search?q={0}";


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings
            , CancellationToken token, Logger info)
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
                client = ClientFactory.GetHttpClient(autoCookies:true).AddHeaders(ClientFactory.FireFoxHeaders);
                CollectCookies(client, token);
            }

            var document = client.GetDoc(searchUrl, token, info);

            var node = document.DocumentNode;
            HtmlNode container = node.SelectSingleNode("//section[@class='products']");

            if (container == null)
            {
                info.WriteLog("[Error] Uncexpected Html!!");
                throw new WebException("Undexpected Html");
            }

            var items = container.SelectChildren("article");



            foreach (var item in items)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item, info);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, item, info);
#endif
            }

            info.State = Logger.ProcessingState.Success;
        }

        /// <summary>
        /// This method is simple wrapper on LoadSingleProduct
        /// To catch all Exceptions during release
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="item"></param>
        /// <param name="info"></param>
        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item, Logger info)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, item, info);
            }
            catch (Exception e)
            {
                info.WriteLog(e.Message);
            }
        }
        /// <summary>
        /// This method handles single product's creation 
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="item"></param>
        /// <param name="info"></param>
        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item, Logger info)
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
                info.WriteLog("Image Of product couldn't found");
            }
            string id = Path.GetFileName(url);


            Product p = new Product(this.WebsiteName, name, url, price, id, null);
            if (!Utils.SatisfiesCriteria(p, settings)) return;

            p.ImageUrl = imagePath;

            listOfProducts.Add(p);
        }

        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        static void CollectCookies(HttpClient client, CancellationToken token)
        {

            var engine = new Jurassic.ScriptEngine();
            engine.SetGlobalValue("interop", "15");

            var task = client.GetAsync("https://www.off---white.com/en/US/", HttpCompletionOption.ResponseContentRead, token);
            var aa = task.Result.Content.ReadAsStringAsync().Result;

            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.off---white.com/en/US/");
            client.DefaultRequestHeaders.Remove("Accept");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");

            var cc = client.GetAsync("https://www.off---white.com/favicon.ico").Result;
            client.DefaultRequestHeaders.Remove("Accept");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
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

            Thread.Sleep(5000);
            var resultTask = client.GetAsync($"https://www.off---white.com/cdn-cgi/l/chk_jschl?jschl_vc={answer}&pass={pass}&jschl_answer={calc}", token);
            var unused = resultTask.Result.Content.ReadAsStreamAsync().Result;
           
        }
    }
}
