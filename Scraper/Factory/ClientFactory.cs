using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Factory
{
    static class ClientFactory
    {
     
        private static Random random = new Random();

        public static StringPair JsonXmlAcceptHeader = ("Accept", "application/xml, application/json");

        public static StringPair HtmlOnlyHeader = ("Accept", "text/html");

        public static StringPair FirefoxUserAgentHeader = ("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:61.0) Gecko/20100101 Firefox/61.0");

        public static StringPair ChromeUserAgentHeader =
            ("User-Agent",
                @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");

        public static StringPair ChromeAcceptHeader = 
            ("Accept",
            @"text/html, application/xhtml+xml, application/xml;q=0.9, image/webp,image/apng, */*;q=0.8");

        public static StringPair FirefoxAcceptHeader =
            ("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

        public static StringPair[] ChromeHeaders = new StringPair[]
        {
            FirefoxAcceptHeader,
            ChromeUserAgentHeader,
            ("Accept-Language", @"en-US,en;q=0.9"),
            ("Accept-Encoding", "gzip,deflate"),
            ("Cache-Control", "no-cache")
        };

        public static StringPair[] FireFoxHeaders = {
            FirefoxUserAgentHeader,
            FirefoxAcceptHeader,
            ("Accept-Encoding", "gzip,deflate"),
            ("Accept-Language", "en-US,en; q=0.5"),
            ("Cache-Control", "no-cache"),
            ("Pramgma","no-cache"),
            ("Connection", "keep-alive"),
            ("DNT","1"),
            ("Upgrade-Insecure-Requests", "1"),
        };


        public static WebProxy ParseProxy(string proxy)
        {

            var tokens = proxy.Split(':');
            if (tokens.Length > 3)
            {
                var password = tokens[tokens.Length - 1];
                var userName = tokens[tokens.Length - 2];
                var address = new UriBuilder(string.Join(":", tokens, 0, tokens.Length - 2)).Uri;
                var cred = new NetworkCredential(userName, password);

                return new WebProxy(address, true, null, cred);
            }

            try
            {
                return new WebProxy(proxy);
            }
            catch
            {
                return null;
            }
        }

        public static WebProxy GetRandomProxy()
        {
            if (!AppSettings.Default.UseProxy || AppSettings.Default.Proxies.Count <= 0) return null;
            var proxyStr = AppSettings.Default.Proxies[random.Next(AppSettings.Default.Proxies.Count - 1)];
            return ParseProxy(proxyStr);
        }

        /// <summary>
        /// Gets scrapping optimized http client.
        /// </summary>
        /// <param name="proxy">proxy to user. If not provided, httpclient will use random proxy from setting's proxy group</param>
        /// <param name="autoCookies">Set true when you want client to automatically handle sending and receiving cookies.
        /// When autoCookies is true, you manually add cookies with Addcookies method</param>
        /// <returns></returns>
        public static HttpClient GetProxiedClient(WebProxy proxy = null, bool autoCookies = false)
        {
            proxy = proxy ?? GetRandomProxy();
            return GetHttpClient(proxy, autoCookies);
        }

        public static HttpClient GetHttpClient(WebProxy proxy = null, bool autoCookies = false)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseCookies = autoCookies,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 3,
            };

            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }

            HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
            client.DefaultRequestHeaders.Clear();

            return client;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    }
}
