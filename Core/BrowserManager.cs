using System;
using System.Collections.Concurrent;
using CheckOutBot.Factory;
using OpenQA.Selenium;

namespace CheckOutBot.Core
{
    public class BrowserManager
    {
        private static readonly Lazy<BrowserManager> Lazy =
            new Lazy<BrowserManager>(() => new BrowserManager());

        public static BrowserManager Instance => Lazy.Value;

        private readonly BlockingCollection<IWebDriver> _queue;

        private BrowserManager()
        {
            _queue = new BlockingCollection<IWebDriver>();
            for (int i = 0; i < AppSettings.InitialBrowserCount; i++)
            {
                _queue.Add(ClientFactory.GetChromeDriver());
            }
        }

        /**
         * Gets driver from pool
         */
        public IWebDriver GetDriver()
        {
            return _queue.Take();
        }

        /**
         * Puts driver back in pool
         */
        public void RetrieveDriver(IWebDriver driver)
        {
            _queue.Add(driver);
        }

    }
}