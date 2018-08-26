using System;
using AkamaiBypasser;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using StoreScraper.Http.Factory;

namespace CheckoutBot.Factory
{
    public class DriverFactory
    {
         /// <summary>
        /// Creates FootSites web-scrapping optimized Firefox driver. proxy is randomly chosen from application proxy pool
        /// </summary>
        /// <param name="showWindowInDebugMode">when set to true browser window will be shown in debug mode.</param>
        /// <returns></returns>
        [Obsolete("Still in development process. Not recommended to use now")]
        public static FirefoxDriver CreateProxiedFirefoxDriver(bool showWindowInDebugMode = true)
        {
            var proxy = ClientFactory.GetRandomProxy();
            return CreateFirefoxDriver(proxy, showWindowInDebugMode);
        }

        /// <summary>
        /// Creates FootSites web-scrapping optimized Firefox driver
        /// </summary>
        /// <param name="proxy">proxy to use. null means no proxy</param>
        /// <param name="showWindowInDebugMode">when set to true browser window will be shown in debug mode.</param>
        /// <returns></returns>
        public static FirefoxDriver CreateFirefoxDriver(System.Net.WebProxy proxy = null, bool showWindowInDebugMode = true)
        {
            
            var options = new ExtendedFirefoxOptions()
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
            options.SetPreference("browser.cache.memory.enable", false);
            options.SetPreference("browser.cache.disk.enable", false);
            options.SetPreference("browser.cache.offline.enable", false);
            options.SetPreference("network.http.use-cache", false);
            options.SetPreference("privacy.trackingprotection.enabled", false);

#if DEBUG
            if (!showWindowInDebugMode) options.AddArgument("-headless");
#else
            options.AddArgument("-headless");
#endif
            var result = new ExtendedFireFoxDriver(options);
            result.Manage().Window.Maximize();
            return result;
        }
    }
}