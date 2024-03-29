﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Data;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;
using Cookie = System.Net.Cookie;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Image = System.Drawing.Image;

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
                    var doc = new HtmlDocument()
                    {
                        OptionAutoCloseOnEnd = true,
                        OptionCheckSyntax = true,
                        OptionFixNestedTags = true,
                        OptionWriteEmptyNodes = true,
                    };
                    doc.LoadHtml(result);
                    response.Dispose();
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
                        var doc = new HtmlDocument()
                        {
                            OptionAutoCloseOnEnd = true,
                            OptionCheckSyntax = true,
                            OptionFixNestedTags = true,
                            OptionWriteEmptyNodes = true,
                        };
                        doc.LoadHtml(result);
                        response.Dispose();
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
                    string result = null;

                    try
                    {
                        result = response.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception e)
                    {
                        if (e is OperationCanceledException && !token.IsCancellationRequested)
                        {
                            throw new WebException("Timeout Exceed. Server did not respond");
                        }

                        throw;
                    }
                    var doc = new HtmlDocument()
                    {
                        OptionAutoCloseOnEnd = true,
                        OptionCheckSyntax = true,
                        OptionFixNestedTags = true,
                        OptionWriteEmptyNodes = true,
                    };
                    doc.LoadHtml(result);
                    response.Dispose();
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


        public static void AddHeaders(this HttpRequestHeaders container, params StringPair[] headers)
        {
            foreach (var header in headers)
            {
                container.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        
        
        public static async Task<List<HtmlNode>> GetPageTask(List<string> urls, CancellationToken token)
        {
            var res = new List<HtmlNode>();
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);

            var documents = await Task.WhenAll(urls.Select(i => client.GetDocTask(i, token)));
            foreach (var document in documents) res.Add(document.DocumentNode);

            return res;
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
        /// Converts relative url to absolute url.
        /// If url is already absolute then just returns it unchanged
        /// </summary>
        /// <param name="url">url to convert</param>
        /// <param name="baseUrl">base Url on which current url will be added</param>
        /// <returns></returns>
        public static string ConvertToFullUrl(this string url, string baseUrl)
        {
            var baseUri = new Uri(baseUrl);
            var currUri = new Uri(url, UriKind.RelativeOrAbsolute);

            return currUri.IsAbsoluteUri ? url : new Uri(baseUri, currUri).AbsoluteUri;
        }

        /// <summary>
        /// This function finds substring of string
        /// From <c>l</c> to <c>r</c> both inclusive
        /// </summary>
        public static string Substr(this string str, int l, int r)
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
            if (settingsBase == null) return true;

            var fullNameLower = product.Name.ToLower() + " " + (product.BrandName?.ToLower() ?? " ") + (product.KeyWords?.ToLower() ?? "");

            if (settingsBase.Mode == SearchMode.NewArrivalsPage &&
                settingsBase.ParsedNegKeywords != null &&
                !settingsBase.ParsedKeywords.Any(kGroup => kGroup.All(keyword => fullNameLower.Contains(keyword))))
            {
                return false;
            }

            if (settingsBase.NegKeyWords != "" && (settingsBase.ParsedNegKeywords?.Any(kGroup => kGroup.All(keyword => fullNameLower.Contains(keyword)))?? false))
            {
                return false;
            }

            if (Math.Abs(settingsBase.MaxPrice) < 0.000001) return true;
            return (Math.Abs(product.Price - (-1)) < 0.000001 || (product.Price <= settingsBase.MaxPrice && product.Price >= settingsBase.MinPrice));
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

        #region Other
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
                    yield return (T)Activator.CreateInstance(type);
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
                    break;
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


        public static string GetMessage(this Exception e)
        {
            while (e is AggregateException aggregate && aggregate.InnerException != null)
            {
                e = aggregate.InnerException;
            }

            return e.Message;
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
        #endregion
    }
}