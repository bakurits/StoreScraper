using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Models;
using Cookie = OpenQA.Selenium.Cookie;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace StoreScraper.Helpers
{
    static public class Utils
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

        public static string EscapeFileName(this string fileName)
        {

            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }


        public static JObject GetParsedJson(this HttpClient client, string url, CancellationToken token)
        {
            var task = client.GetStringAsync(url);
            task.Wait(token);

            if (task.IsFaulted)
            {
                throw new JsonException("Can't parse json info");
            }

            var buyOptions = JObject.Parse(task.Result);

            return buyOptions;
        }


        public static HtmlDocument GetDoc(this HttpClient client, string url, CancellationToken token)
        {
            try
            {
                using (var response = client.GetAsync(url, token).Result)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(result);
                    return doc;
                }
            }
            catch (WebException)
            {
                Logger.Instance.WriteErrorLog("Can't connect to website");
                throw;
            }
        }
        
        public static HtmlDocument PostDoc(this HttpClient client, string url, CancellationToken token, FormUrlEncodedContent postParams)
        {
            
            try
            {
                using (var response = client.PostAsync(url, postParams, token).Result)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(result);
                    return doc;
                }
            }
            catch (WebException)
            {
                Logger.Instance.WriteErrorLog("Can't connect to website");
                throw;
            }
        }
        

        public static HtmlDocument GetDoc(Func<HttpClient> clientGenerator, string url, int timeoutSeconds, int maxTries, 
            CancellationToken token, bool autoDispose = false)
        {
            for (int i = 0; i < maxTries; i++)
            {
                var client = clientGenerator();
                try
                {
                    return client.GetDoc(url, token);
                }
                catch(Exception e)
                {
                    if (i == maxTries - 1)
                    {
                        Logger.Instance.WriteErrorLog($"Can't connect to webiste url: {url}. ErrorMessage: {e.Message}");
                        throw;
                    }
                }
                finally
                {
                    if(autoDispose) client.Dispose();
                }

            }

            Logger.Instance.WriteErrorLog($"Can't connect to webiste url: {url}");
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

        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.Invoke((MethodInvoker) delegate
            {
                box.SelectionStart = box.TextLength;
                box.SelectionLength = 0;

                box.SelectionColor = color;
                box.AppendText(text);
                box.SelectionColor = box.ForeColor;
            });

        
        }

        private static Random r = new Random();

        public static T GetRandomValue<T>(this IList<T> list)
        {
            int index = r.Next(0, list.Count);
            return list[index];
        }


        /// <summary>
        /// Method return currency string by recognising
        /// the first charracter in the price string
        /// consider adding more currency strings
        /// in the cases
        /// </summary>
        /// <param name="priceString"></param>
        /// <returns>List of sizes</returns>
        public static Price ParsePrice (string priceString, string decimalDelimiter = ".", string tousandsDelimiter = ",")
        {

            priceString = priceString.Trim().Replace(" ", "");
            if (!string.IsNullOrEmpty(tousandsDelimiter)) priceString = priceString.Replace(tousandsDelimiter, "");

            priceString = priceString.Replace(decimalDelimiter, ".");

            string number = Regex.Match(priceString, $@"[\d\.]+").Value;
            var parsed = double.Parse(number, CultureInfo.InvariantCulture);
            var c = priceString.Replace(number, "").ToUpper();

            c = c == "$" ? "USD" : c == "€" ? "EUR" : c;

            return new Price(parsed ,c);
        }
    }
}
