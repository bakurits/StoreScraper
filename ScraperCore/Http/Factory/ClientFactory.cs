using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using ScraperCore.Http;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Http.Factory
{

    public static class ClientFactory
    {

        private static Random random = new Random();

        public static StringPair JsonXmlAcceptHeader = ("Accept", "application/xml, application/json");

        public static StringPair JsonAcceptHeader = ("Accept", "application/json");

        public static StringPair HtmlOnlyHeader = ("Accept", "text/html");

        public static StringPair FirefoxUserAgentHeader = ("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:61.0) Gecko/20100101 Firefox/61.0");

        public static StringPair FirefoxUserAgentHeaderOlder = ("User-Agent",
            "Mozilla/5.0 (Windows NT 6.1; rv:52.0) Gecko/20100101 Firefox/52.0");

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


        public static StringPair[] EdgeHeaders = 
        {
            ("Accept", "*/*"),
            ("Accept-Encoding","gzip, deflate, br"),
            ("Accept-Language", "en-US; q=0.7, en; q=0.3"),
            ("Cache-Control", "no-cache"),
            ("Connection", "Keep-Alive"),
            ("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134")
        };

        public static StringPair[] FireFoxHeaders =
        {
            FirefoxUserAgentHeader,
            FirefoxAcceptHeader,
            ("Accept-Encoding", "gzip, deflate, br"),
            ("Accept-Language", "en-US,en; q=0.5"),
            ("Cache-Control", "no-cache"),
            ("Pramgma","no-cache"),
            ("Connection", "keep-alive"),
            //("DNT","1"),
            ("Upgrade-Insecure-Requests", "1"),
        };

        public static StringPair[] DefaultHeaders = FireFoxHeaders;


        public static StringPair[] FireFoxHeaders2 =
        {
            FirefoxUserAgentHeader,
            FirefoxAcceptHeader,
            ("Accept-Encoding", "gzip,deflate"),
            ("Accept-Language", "en-US,en; q=0.5"),
            ("Cache-Control", "no-cache"),
            ("Pramgma","no-cache"),
            ("Connection", "close"),
            ("DNT","1"),
            ("Upgrade-Insecure-Requests", "1"),
        };


        public static HttpClient GeneralClient = new HttpClient();

        public static WebProxy ParseProxy(string proxy)
        {

            try
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
            }
            catch
            {
                //
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


        public static FirefoxDriver CreateProxiedFirefoxDriver(bool showWindowInDebugMode = true)
        {
            var proxy = GetRandomProxy();

            FirefoxOptions options = new FirefoxOptions()
            {
                AcceptInsecureCertificates = true,
            };

            if (proxy != null)
            {
                options.Proxy = new Proxy()
                {
                    IsAutoDetect = false,
                    Kind = ProxyKind.Manual,
                    HttpProxy = proxy.Address.AbsoluteUri,
                    SslProxy = proxy.Address.AbsoluteUri
                };
            }

            options.AddArguments("-private", "-new-instance");

#if DEBUG
            if (!showWindowInDebugMode) options.AddArgument("-headless");
#else
            options.AddArgument("-headless");
#endif
            var result = new FirefoxDriver(options);
            result.Manage().Window.Maximize();
            return result;
        }


        
        public static ChromeDriver CreateProxiedChromeDriver(bool showInDebugMode = true, bool ingocgnito = true)
        {
            var proxy = GetRandomProxy();

            ChromeOptions options = new ChromeOptions()
            {
                AcceptInsecureCertificates = true
            };    
            
            if (proxy != null)
            {
                options.Proxy = new Proxy()
                {
                    IsAutoDetect = false,
                    Kind = ProxyKind.Manual,
                    HttpProxy = proxy.Address.AbsoluteUri,
                    SslProxy = proxy.Address.AbsoluteUri
                };
            }
            options.AddArguments("--disable-infobars", "--start-maximized", "--disable-plugins-discovery");

            if (ingocgnito)
            {
                options.AddArgument("--incognito");
            }

#if DEBUG
            if (!showInDebugMode)
            {
                options.AddArgument("--headless");
            }
#else
            options.AddArgument("-headless");
#endif

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();            

            var result =  new ChromeDriver(service, options);
            return result;
        }


        public static ChromeDriver CreateMobileChromeDriver(bool showInDebugMode = true)
        {
            var proxy = GetRandomProxy();

            ChromeOptions options = new ChromeOptions()
            {
                AcceptInsecureCertificates = true
            };    
            
            if (proxy != null)
            {
                options.Proxy = new Proxy()
                {
                    IsAutoDetect = false,
                    Kind = ProxyKind.Manual,
                    HttpProxy = proxy.Address.AbsoluteUri,
                    SslProxy = proxy.Address.AbsoluteUri
                };
            }

            options.EnableMobileEmulation("iPad");

            options.AddArguments("--incognito", "--start-maximized", "--profile-directory=Default", "--disable-extensions", "--disable-plugins-discovery");

#if DEBUG
            if (!showInDebugMode) options.AddArgument("--headless");
#else
            options.AddArgument("-headless");
#endif

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();            

            return new ChromeDriver(service, options);
        }

        /// <summary>
        /// Gets scrapping optimized http client.
        /// </summary>
        /// <param name="proxy">proxy to user. If not provided, httpclient will use random proxy from setting's proxy group</param>
        /// <param name="autoCookies">Set true when you want client to automatically handle sending and receiving cookies.
        /// When autoCookies is true, you manually add cookies with Addcookies method</param>
        /// <returns></returns>
        public static HttpClient GetProxiedFirefoxClient(WebProxy proxy = null, bool autoCookies = true)
        {
            proxy = proxy ?? GetRandomProxy();
            return GetFirefoxHttpClient(proxy, autoCookies);
        }

        public static HttpClient CreateProxiedHttpClient(WebProxy proxy = null, bool autoCookies = false)
        {
            proxy = proxy ?? GetRandomProxy();
            return CreateHttpCLient(proxy, autoCookies);
        }

        public static HttpClient CreateHttpCLient(WebProxy proxy = null, bool autoCookies = false)
        {
            HttpClientHandler handler = new ExtendedClientHandler()
            {
                UseCookies = autoCookies,
                MaxAutomaticRedirections = 3,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                AllowAutoRedirect = true,
            };


            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }

            HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
            return client;
        }

        public static FirefoxHttpClientStorage Storage = new FirefoxHttpClientStorage();

        public static HttpClient GetFirefoxHttpClient(WebProxy proxy = null, bool autoCookies = false)
        {
            if (!autoCookies)
            {
                return CreateProxiedHttpClient(proxy).AddHeaders(DefaultHeaders);
            }

            return proxy == null ? Storage.GetHttpClient() : Storage.GetHttpClient(proxy);
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    }
}
