using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Helpers;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Core
{
    public class ProductMonitoringManager
    {
        public delegate void NewProductHandler(Product product);


        private class ShopMonitoringTask
        {
            public HashSet<Product> CurrentProducts { get; set; }
            public event NewProductHandler Handler;
            public CancellationTokenSource TokenSource { get; set; }
            public TimeSpan MonitoringInterval { get; set; }

            public void OnNewProductAppeared(Product product)
            {
                Handler?.Invoke(product);
            }
        }


        public static ProductMonitoringManager Default { get; set; } = new ProductMonitoringManager(Logger.Instance);

        private Dictionary<ScraperBase, ShopMonitoringTask> _allProductsCollection { get; set; } = new Dictionary<ScraperBase, ShopMonitoringTask>();
        private Logger _logger { get; }


        public ProductMonitoringManager(Logger logger)
        {
            this._logger = logger;
        }

        public void AddNewProductHandler(ScraperBase website, NewProductHandler handler)
        {
            if (_allProductsCollection.TryGetValue(website, out var task))
            {
                task.Handler += handler;
            }
            else
            {
                throw new KeyNotFoundException($"Can't add new product handler to {website.WebsiteName}, \n because monitoring on this website not runnning");
            }
        }

        public void RegisterMonitoringTask(ScraperBase website, TimeSpan monInterval, CancellationToken token)
        {
            var curList = ScrapeCurrentProductsList(website, token);

            var task = new ShopMonitoringTask()
            {
                CurrentProducts = curList,
                MonitoringInterval = monInterval,
                TokenSource = new CancellationTokenSource(),
            };

            _allProductsCollection.Add(website, task);
            Task.Factory.StartNew(() => MonitorShop(website, task), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void RemoveMonitoringTask(ScraperBase website)
        {
            if(!_allProductsCollection.TryGetValue(website, out var task)) throw new KeyNotFoundException($"can't remove {website} monitoring, because it is not registered");

            task.TokenSource.Cancel();
            _allProductsCollection.Remove(website);
        }

        public IReadOnlyCollection<Product> GetCurrentProducts(ScraperBase website)
        {
            if (_allProductsCollection.TryGetValue(website, out var task))
            {
                return task.CurrentProducts.ToList().AsReadOnly();
            }
            else
            {
                throw new KeyNotFoundException($"Can't retrieve product list of {website.WebsiteName}, \n because monitoring on this website not runnning");
            }
        }

        private HashSet<Product> ScrapeCurrentProductsList(ScraperBase website, CancellationToken token)
        {
            List<Product> products = new List<Product>();
            Utils.TrySeveralTimes(() => website.ScrapeAllProducts(out products, ScrappingLevel.Url, token), AppSettings.Default.ProxyRotationRetryCount);


            return new HashSet<Product>(products);
        }


        private void MonitorShop(ScraperBase website, ShopMonitoringTask task)
        {
            Stopwatch counter = Stopwatch.StartNew();

            while (!task.TokenSource.Token.IsCancellationRequested)
            {
                counter.Restart();
                HashSet<Product> products = null;

                try
                {
                    products = ScrapeCurrentProductsList(website, task.TokenSource.Token);
                }
                catch (Exception e)
                {
                    _logger.WriteErrorLog($"Error while scrapping {website}. \n msg= {e.Message} \n");
                }


                if (products != null)
                {

                    lock (task.CurrentProducts)
                    {
                        foreach (var product in products)
                        {
                            if (task.CurrentProducts.Contains(product)) continue;
                            task.CurrentProducts.Add(product);
                            Task.Run(() =>
                            {
                                try
                                {
                                    task.OnNewProductAppeared(product);
                                    _logger.WriteVerboseLog($"Succesfully invoker new product event handler");
                                }
                                catch (Exception e)
                                {
                                    _logger.WriteErrorLog($"Error while invoking new product event handler. \n msg={e.Message} \n");
                                }
                            });
                        }
                    } 
                }

                if (task.MonitoringInterval > counter.Elapsed)
                {
                    Task.Delay(task.MonitoringInterval - counter.Elapsed, task.TokenSource.Token)
                        .Wait(task.TokenSource.Token);
                }
            }
        }
    }
}
