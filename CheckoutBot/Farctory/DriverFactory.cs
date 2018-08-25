using System;
using AkamaiBypasser;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using StoreScraper.Http.Factory;

namespace CheckoutBot.Farctory
{
    public class DriverFactory
    {
         /// <summary>
        /// Cretes FootSites webscrapping optimized Firefox driver. proxy is randomly chosen from application proxy pool
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
        /// Cretes FootSites webscrapping optimized Firefox driver
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

            const string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";
            options.AddArguments("-private", "-new-instance", $"--useragent={UserAgent}");

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