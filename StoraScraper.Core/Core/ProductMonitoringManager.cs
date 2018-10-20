using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Helpers;
using StoreScraper.Models;
using StoreScraper.Models.Enums;
#pragma warning disable 4014

namespace StoreScraper.Core
{
    public class ProductMonitoringManager
    {
        public delegate void NewProductHandler(Product product);


        private class ShopMonitoringTask
        {
            public ScraperBase Website { private get; set; }
            public HashSet<Product> CurrentProducts { get; set; }
            public event NewProductHandler Handler;
            public CancellationTokenSource TokenSource { get; set; }
            private bool _started = false;

            private void OnNewProductAppeared(Product product)
            {
                Handler?.Invoke(product);
            }

            public void Start()
            {
                if(_started)return;
                Task.Factory.StartNew(MonitorShop, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _started = true;
            }

            private void MonitorShop()
            {


                while (!TokenSource.Token.IsCancellationRequested)
                {
                    var counter = Stopwatch.StartNew();
                    HashSet<Product> products = null;

                    try
                    {
                        Stopwatch stopWatch = Stopwatch.StartNew();
                        products = ScrapeCurrentProductsList(Website, TokenSource.Token);
                        Logger.Instance.WriteVerboseLog($"Successfully scraped {Website}. Scrapping took {stopWatch.ElapsedMilliseconds}ms !!!");
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.WriteErrorLog($"Error while scrapping {Website}. \n msg= {e.Message} \n");
                    }


                    if (products != null)
                    {

                        lock (CurrentProducts)
                        {
                            foreach (var product in products)
                            {
                                if (CurrentProducts.Contains(product)) continue;
                                CurrentProducts.Add(product);
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        OnNewProductAppeared(product);
                                        Logger.Instance.WriteVerboseLog($"Successfully invoked new product event handler");
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Instance.WriteErrorLog($"Error while invoking new product event handler. \n msg={e.Message} \n");
                                    }
                                });
                            }
                        }
                    }

                    if (AppSettings.Default.MonitoringInterval > counter.ElapsedMilliseconds)
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(AppSettings.Default.MonitoringInterval - counter.ElapsedMilliseconds), TokenSource.Token)
                            .Wait(TokenSource.Token);
                    }
                }
            }
        }


        public static ProductMonitoringManager Default { get; set; } = new ProductMonitoringManager(Logger.Instance);

        private Dictionary<ScraperBase, ShopMonitoringTask> _allShopsTasks { get; set; } = new Dictionary<ScraperBase, ShopMonitoringTask>();
        private Logger _logger { get; }


        public ProductMonitoringManager(Logger logger)
        {
            this._logger = logger;
        }

        public void AddNewProductHandler(ScraperBase website, NewProductHandler handler)
        {
            if (_allShopsTasks.TryGetValue(website, out var task))
            {
                task.Handler += handler;
            }
            else
            {
                throw new KeyNotFoundException($"Can't add new product handler to {website.WebsiteName}, \n because monitoring on this website not running");
            }
        }

        public async Task RegisterMonitoringTaskAsync(ScraperBase website, CancellationToken token)
        {
            if (_allShopsTasks.ContainsKey(website))
            {
                _logger.WriteVerboseLog("Requested shop monitoring task is already added (Adding Process Skipped)");
                return;
            }

            var curList = await Task.Run(() => ScrapeCurrentProductsList(website, token));

            var task = new ShopMonitoringTask()
            {   
                Website = website,
                CurrentProducts = curList,
                TokenSource = new CancellationTokenSource(),
            };
             
            _allShopsTasks.Add(website, task);
        }


        public void StartMonitoringTask(ScraperBase website)
        {
            if(_allShopsTasks.TryGetValue(website, out var task)) throw new KeyNotFoundException("Can't start task. Task with this name not found");

            task.Start();
        }

        public void RemoveMonitoringTask(ScraperBase website)
        {
            if (!_allShopsTasks.TryGetValue(website, out var task)) throw new KeyNotFoundException($"Can't remove {website} monitoring, because it is not registered");

            task.TokenSource.Cancel();
            _allShopsTasks.Remove(website);
        }

        public IReadOnlyCollection<Product> GetCurrentProducts(ScraperBase website)
        {
            if (_allShopsTasks.TryGetValue(website, out var task))
            {
                return task.CurrentProducts.ToList().AsReadOnly();
            }
            else
            {
                throw new KeyNotFoundException($"Can't retrieve product list of {website.WebsiteName}, \n because monitoring on this website not runnning");
            }
        }

        private static HashSet<Product> ScrapeCurrentProductsList(ScraperBase website, CancellationToken token)
        {
            List<Product> products = new List<Product>();
            Utils.TrySeveralTimes(() => website.ScrapeAllProducts(out products, ScrappingLevel.Url, token), AppSettings.Default.ProxyRotationRetryCount);


            return new HashSet<Product>(products);
        }

    }
}
