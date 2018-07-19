using System;
using System.Net;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace StoreScraper.Factory
{
    class ClientFactory
    {
     
        private static Random random = new Random();

        public static object ChromeHeaders = new
        {
            Accept =
                @"text/html, application/xhtml+xml, application/xml;q=0.9, image/webp,image/apng, */*;q=0.8",
            User_Agent =
                @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36",
            Accept_Language = @"en-US,en;q=0.9",
            Accept_Encoding = "gzip,deflate",
            Cache_Control = "no-cache"
        };


        public static IWebDriver GetChromeDriver()
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

        private static WebProxy ParseProxy(string proxy)
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
                return new WebProxy(proxy);
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

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    }
}
