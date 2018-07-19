using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckOutBot.Factory;
using CheckOutBot.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using OpenQA.Selenium;
using Cookie = OpenQA.Selenium.Cookie;

namespace CheckOutBot
{
    static class Utils
    {
        public static IEnumerable<HtmlNode> SelectChildren(this HtmlNode parent, string tagName)
        {
            foreach (var child in parent.ChildNodes)
            {
                if (child.Name == tagName)
                {
                    yield return child;
                }            
            }
        }

        /// <summary>
        /// Gets Image from specified Url
        /// And resizes it to supplied width, height
        /// </summary>
        public static Image GetImage(this IFlurlRequest client, int width, int height)
        {
            var task = client.GetStreamAsync();
            task.Wait();

            using (var stream = task.Result)
            {
                return new Bitmap(Image.FromStream(stream, false, true), width, height);
            }
        }

        public static bool SatisfiesCriteria(Product product, SearchSettingsBase settingsBase)
        {
            var negKeyWords = settingsBase.NegKeyWrods.ToLower().Split(' ').ToList();
            var lName = product.Name.ToLower();

            if (negKeyWords[0] != "" && negKeyWords.Find(word => lName.Contains(word)) != null) return false;

            if (settingsBase.MaxPrice == 0) return true;
            return product.Price <= settingsBase.MaxPrice && product.Price >= settingsBase.MinPrice;
        }


        public static JObject GetParsedJson(this HttpClient client, string url, CancellationToken token)
        {
            var task = client.GetStringAsync(url);
            task.Wait(token);

            if (task.IsFaulted)
            {
                throw new JsonException("Can't parse json info");
            }

            JObject buyOptions = JObject.Parse(task.Result);

            return buyOptions;
        }


        public static HtmlDocument GetDoc(this HttpClient client, string url, CancellationToken token)
        {
            var task = client.GetStringAsync(url);
            HtmlDocument doc = new HtmlDocument();
            task.Wait(token);
            token.ThrowIfCancellationRequested();
            var str = task.Result;
            doc.LoadHtml(str);

            return doc;
        }

        public static HtmlDocument GetDoc(this IFlurlRequest request, CancellationToken token)
        {
            var task = request.GetStringAsync();
            task.Wait(token);
            token.ThrowIfCancellationRequested();
            var result = task.Result;

            var doc = new HtmlDocument();
            doc.LoadHtml(result);

            return doc;
        }

        public static IFlurlRequest WithCookies(this IFlurlRequest request, IEnumerable<Cookie> cookies)
        {
            IFlurlRequest result = request;

            foreach (var cookie in cookies)
            {
                result = result.WithCookie(cookie.Name, cookie.Value, cookie.Expiry);
            }

            return result;
        }

        public static async Task<IEnumerable<Cookie>> SolveAntiBotChallenge(this IWebDriver browser, string url, CancellationToken token)
        {
            await Task.Run(() => browser.Navigate().GoToUrl(url), token);

            await Task.Delay(8000, token);
            token.ThrowIfCancellationRequested();

            return browser.Manage().Cookies.AllCookies.ToList();
        }

        public static IFlurlRequest WithProxy(this string url)
        {
            
            var proxy = ClientFactory.GetRandomProxy();
            var request = new FlurlRequest(url);
            if (proxy == null) return request;

            request.Client =  request.Client.Configure(setting => 
                setting.HttpClientFactory = new ProxyHttpClientFactory(proxy.Address.AbsoluteUri));

            return request;
        }

        /// <summary>
        /// Adds Proxy in FurlRequest.
        /// proxy is added only if useProxy is set to true in settings
        /// </summary>
        /// <returns></returns>
        public static IFlurlRequest WithProxy(this string url, string proxy)
        {
            var request = new FlurlRequest(url);
            if (!AppSettings.Default.UseProxy) return request;

            request.Client = request.Client.Configure(setting =>
                setting.HttpClientFactory = new DefaultHttpClientFactory());

            return request;
        }
    }
}
