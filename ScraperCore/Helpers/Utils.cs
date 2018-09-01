using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using ScraperCore.Models;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using Cookie = System.Net.Cookie;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace StoreScraper.Helpers
{
    public static class Utils
    {
        
        private static readonly Random R = new Random();
        
        #region HttpClient

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

        public static async Task<HtmlDocument> GetDocTask(this HttpClient client, string url, CancellationToken token)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(url, token))
                {
                    string v = await response.Content.ReadAsStringAsync();
                    var result = v;
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

        public static HtmlDocument GetDoc(this HttpClient client, HttpRequestMessage message, CancellationToken token)
        {
            try
            {
                using (var response = client.SendAsync(message, token).Result)
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

        public static HtmlDocument PostDoc(this HttpClient client, string url, CancellationToken token,
            FormUrlEncodedContent postParams)
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


        public static HtmlDocument GetDoc(Func<HttpClient> clientGenerator, string url, int timeoutSeconds,
            int maxTries,
            CancellationToken token, bool autoDispose = false)
        {
            for (int i = 0; i < maxTries; i++)
            {
                var client = clientGenerator();
                try
                {
                    return client.GetDoc(url, token);
                }
                catch (Exception e)
                {
                    if (i == maxTries - 1)
                    {
                        Logger.Instance.WriteErrorLog(
                            $"Can't connect to website url: {url}. ErrorMessage: {e.Message}");
                        throw;
                    }
                }
                finally
                {
                    if (autoDispose) client.Dispose();
                }
            }

            Logger.Instance.WriteErrorLog($"Can't connect to website url: {url}");
            throw new WebException($"Can't connect to website url: {url}");
        }

        public static HttpClient AddCookies(this HttpClient client, params Cookie[] cookies)
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

        #endregion

        #region Strings

        public static string EscapeFileName(this string fileName)
        {
            return System.IO.Path.GetInvalidFileNameChars()
                .Aggregate(fileName, (current, c) => current.Replace(c, '_'));
        }

        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            return !(Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute
                )
                ? value.ToString()
                : attribute.Description;
        }

        /// <summary>
        /// This function removes all new line characters from string
        /// </summary>
        /// <param name="str"> Give string </param>
        /// <returns> string without new line characters </returns>
        public static string EscapeNewLines(this string str)
        {
            return Regex.Replace(str, @"\t|\n|\r", "");
        }

        /// <summary>
        /// This function finds substring of string
        /// From <c>l</c> to <c>r</c> both inclusive
        /// </summary>
        private static string Substr(this string str, int l, int r)
        {
            return str.Substring(l, r - l + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HtmlDeEntitize(this string str)
        {
            return HtmlEntity.DeEntitize(str);
        }

        /// <summary>
        /// This function finds string before some pattern
        /// </summary>
        /// <example>
        /// "abcd/ab".StringBefore("/ab") = "abcd"
        /// </example>
        /// <returns>String before pattern</returns>
        public static string StringBefore(this string str, string pattern)
        {
            int indx = str.LastIndexOf(pattern, StringComparison.Ordinal);
            if (indx != -1)
            {
                return str.Substr(0, indx);
            }

            return str;
        }

        #endregion

        #region JsonUtils

        public static JObject GetFirstJson(string str)
        {
            int firstCrlBraInd = str.IndexOf("{", StringComparison.Ordinal);
            if (firstCrlBraInd == -1)
            {
                return JObject.Parse("{}");
            }

            int cnt = 1;
            for (int i = firstCrlBraInd + 1; i < str.Length; i++)
            {
                if (str[i] == '{') cnt++;
                if (str[i] == '}') cnt--;
                if (cnt == 0)
                {
                    return JObject.Parse(str.Substr(firstCrlBraInd, i));
                }
            }

            return JObject.Parse("{}");
        }

        public static string ToJsonString(this object value)
        {
            return JsonConvert.SerializeObject(value,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }

        #endregion

        #region Scraping

        public static bool SatisfiesCriteria(Product product, SearchSettingsBase settingsBase)
        {
            var negKeyWords = settingsBase.NegKeyWords.ToLower().Split(' ').ToList();
            var lName = product.Name.ToLower();

            if (settingsBase.Mode == SearchMode.NewArrivalsPage &&
                !settingsBase.parsedKeywords.Any(kGroup => kGroup.All(keyword => product.Name.Contains(keyword))))
            {
                return false;
            }

            if (negKeyWords[0] != "" && negKeyWords.Find(word => lName.Contains(word)) != null) return false;

            if (Math.Abs(settingsBase.MaxPrice) < 0.000001) return true;
            return product.Price <= settingsBase.MaxPrice && product.Price >= settingsBase.MinPrice;
        }

        private static readonly Dictionary<string, string> CurrencyConversionSet = new Dictionary<string, string>()
        {
            {"USD", "$"},
            {"&#36", "$"},
            {"EUR", "€"},
            {"&EURO", "€"},
            {"&POUND", "£"},
            {"&YEN", "¥"},
        };


        /// <summary>
        /// Method return currency string by recognizing
        /// the first character in the price string
        /// consider adding more currency strings
        /// in the cases
        /// </summary>
        /// <param name="priceString"></param>
        /// <param name="decimalDelimiter"></param>
        /// <param name="thousandsDelimiter"></param>
        /// <returns>List of sizes</returns>
        public static Price ParsePrice(string priceString, string decimalDelimiter = ".",
            string thousandsDelimiter = ",")
        {
            priceString = priceString.Trim().Replace(" ", "");
            if (!string.IsNullOrEmpty(thousandsDelimiter)) priceString = priceString.Replace(thousandsDelimiter, "");
            priceString = HtmlEntity.DeEntitize(priceString);
            priceString = priceString.Replace(decimalDelimiter, ".");

            string number = Regex.Match(priceString, $@"[\d\.]+").Value;
            var parsed = double.Parse(number, CultureInfo.InvariantCulture);
            var c = priceString.Replace(number, "").ToUpper();

            if (CurrencyConversionSet.ContainsKey(c)) c = CurrencyConversionSet[c];

#if DEBUG
            if (c.Any(char.IsNumber))
                Logger.Instance.WriteErrorLog($"Couldn't parse string to price. str = {priceString}");
#endif

            return new Price(parsed, c);
        }

        #endregion

        #region BrowserSimulation

        public static void ClickAtRandomPoint(this IWebDriver driver, IWebElement elem)
        {
            Actions actions = new Actions(driver);
            actions.MoveToElement(elem, R.Next(0, elem.Size.Width), R.Next(0, elem.Size.Height));
            actions.Click();
            actions.Build().Perform();
        }

        public static void SimulateTyping(this IWebDriver driver, string keysToType)
        {
            Actions actions = new Actions(driver);
            actions.SendKeys(keysToType).Build().Perform();
        }

        #endregion


        

        public static T GetRandomValue<T>(this IList<T> list)
        {
            int index = R.Next(0, list.Count);
            return list[index];
        }


        public static IEnumerable<T> GetAllSubClassInstances<T>()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(T))) continue;
                bool disabled = type.CustomAttributes.Any(attr => attr.AttributeType == typeof(DisableInGUI));
                if (!disabled)
                {
                    yield return (T) Activator.CreateInstance(type);
                }
            }
        }


        public static void TrySeveralTimes(Action action, int attemptCount)
        {
            for (int i = 0; i < attemptCount; i++)
            {
                try
                {
                    action();
                }
                catch
                {
                    if (i == attemptCount - 1) throw;
                }
            }
        }

        public static object TrySeveralTimes(Func<object> action, int attemptCount)
        {
            for (int i = 0; i < attemptCount; i++)
            {
                try
                {
                    return action();
                }
                catch
                {
                    if (i == attemptCount - 1) throw;
                }
            }

            return null;
        }


        public static void WaitToBecomeTrue(this Func<bool> predicate, CancellationToken token,
            int checkIntervalMilliSeconds = 100)
        {
            while (true)
            {
                if (predicate()) return;
                token.ThrowIfCancellationRequested();
                Task.Delay(checkIntervalMilliSeconds, token).Wait(token);
            }
        }
    }
}