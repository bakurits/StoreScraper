using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
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

        public static StringPair[] FireFoxHeaders = new StringPair[]
        {
            ChromeAcceptHeader,
            ("Accept-Encoding", "gzip,deflate"),
            ("Accept-Language", "en-US,en; q=0.5"),
            ("Cache-Control", "no-cache"),
            ("Connection", "keep-alive"),
            ("Pragma", "no-cache"),
            ("DNT","1"),
            ("Upgrade-Insecure-Requests", "1"),
            FirefoxUserAgentHeader
        };


        public static ChromeDriver GetChromeDriver()
        {
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;
            options.AddArguments("--incognito", "--disable-infobars");
#if !DEBUG
            options.AddArgument("--window-position=-32000,-32000");
#endif
            if (AppSettings.Default.UseProxy && AppSettings.Default.Proxies.Count > 0)
            {
                var webProxy = GetRandomProxy();
                string proxyStr = webProxy.Address.AbsoluteUri.Replace("\n", "");
                var proxy = new Proxy();
                proxy.Kind = ProxyKind.Manual;
                proxy.IsAutoDetect = false;
                proxy.HttpProxy = proxyStr;
                proxy.SslProxy = proxyStr;
                options.Proxy = proxy;
                options.AddArgument("ignore-certificate-errors");
            }

            ChromeDriver driver = new ChromeDriver(service,options);
#if !DEBUG
            driver.Navigate().GoToUrl("chrome://version/");
            var processes = Process.GetProcessesByName("chrome").Where(p => p.MainWindowTitle.Contains(driver.Title));
            foreach (var p in processes)
            {
                ShowWindowAsync(p.MainWindowHandle, 0);
            }
#endif
            return driver;
        }


        public static (FirefoxOptions, FirefoxDriver) GetFirefoxDriver()
        {
            var service = FirefoxDriverService.CreateDefaultService();
            var options = new FirefoxOptions();
            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;

            var proxy = GetRandomProxy();
#if !DEBUG
            options.AddArguments("-headless");
#endif
            if (proxy == null) return (options, new FirefoxDriver(service, options));
            var proxyAddr = proxy.Address.Host + ":" + proxy.Address.Port;
            options.Proxy = new Proxy()
            {
                Kind = ProxyKind.Manual,
                HttpProxy = proxyAddr,
                SslProxy = proxyAddr,
                IsAutoDetect = false,
            };

            return (options, new FirefoxDriver(service,options));
        }

        public static WebProxy ParseProxy(string proxy)
        {

            var tokens = proxy.Split(':');
            if (tokens.Length > 3)
            {
                var password = tokens[tokens.Length - 1];
                var userName = tokens[tokens.Length - 2];
                var address = string.Join(":", tokens, 0, tokens.Length - 2);
                var cred = new NetworkCredential(userName, password);

                return new WebProxy(address, true, null, cred);
            }
            else
            {
                try
                {
                    return new WebProxy(proxy);
                }
                catch
                {
                    return null;
                }

            }
        }

        public static WebProxy GetRandomProxy()
        {
            if (AppSettings.Default.UseProxy && AppSettings.Default.Proxies.Count > 0)
            {
                var proxyStr = AppSettings.Default.Proxies[random.Next(AppSettings.Default.Proxies.Count - 1)];
                return ParseProxy(proxyStr);
            }

            return null;
        }

        /// <summary>
        /// Gets scrapping optimized http client.
        /// </summary>
        /// <param name="proxy">proxy to user. If not provided, httpclient will use random proxy from setting's proxy group</param>
        /// <param name="autoCookies">Set true when you want client to automatically handle sending and receiving cookies.
        /// When autoCookies is true, you manually add cookies with Addcookies method</param>
        /// <returns></returns>
        public static HttpClient GetHttpClient(WebProxy proxy = null, bool autoCookies = false)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseCookies = autoCookies,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 3,
            };

            proxy = proxy ?? GetRandomProxy();

            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }

            HttpClient client = new HttpClient(handler);

            return client;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    }
}
