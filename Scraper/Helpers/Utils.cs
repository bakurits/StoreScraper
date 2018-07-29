using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using StoreScraper.Factory;
using StoreScraper.Models;
using Cookie = OpenQA.Selenium.Cookie;

namespace StoreScraper.Helpers
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
        public static Image GetImage(this HttpClient client, string url, int width, int height)
        {
            var task = client.GetStreamAsync(url);
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


        public static HtmlDocument GetDoc(this HttpClient client, string url, CancellationToken token, Logger logger = null)
        {
            try
            {
                var result = client.GetAsync(url, token).Result.Content.ReadAsStringAsync().Result;
                var doc = new HtmlDocument();
                doc.LoadHtml(result);
                return doc;
            }
            catch (WebException)
            {
                logger?.WriteLog("[Error] Can't connect to website");
                throw;
            }
        }

        public static HtmlDocument GetDoc(Func<HttpClient> clientGenerator, string url, int timeoutSeconds, int maxTries, 
            CancellationToken token, Logger logger = null)
        {
            for (int i = 0; i < maxTries; i++)
            {
                using (var client = clientGenerator())
                {
                    try
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                        var result = client.GetAsync(url, token).Result.Content.ReadAsStringAsync().Result;
                        var doc = new HtmlDocument();
                        doc.LoadHtml(result);
                        return doc;
                    }
                    catch
                    {
                        //ingored
                    }
                }        
            }

            logger?.WriteLog($"[Error] Can't connect to webiste url: {url}");
            throw new WebException($"Can't connect to webiste url: {url}");
        }

        public static HttpClient AddCookies(this HttpClient client, IEnumerable<Cookie> cookies)
        {
            var list = cookies.ToList();
            var cookieStr = string.Join(";", list.ConvertAll(cookie => $"{cookie.Name}={cookie.Value}"));

           
            client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookieStr);

            return client;
        }

        public static HttpClient AddHeaders(this HttpClient client, params StringPair[] headers)
        {
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            return client;
        }
    }
}
