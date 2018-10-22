using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StoreScraper.Data;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    [JsonObject]
    public class ShopMonitoringTask
    {
        [JsonIgnore]
        public ScraperBase Website { private get; set; }

        public string WebsiteUrl
        {
            get => Website.WebsiteBaseUrl;
            set => Website = Session.Current.AvailableScrapers.Find(scraper => scraper.WebsiteBaseUrl == value);
        }

        public HashSet<Product> CurrentProducts { get; set; }

        public event ProductMonitoringManager.NewProductHandler Handler;
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
        private bool _started = false;

        private void OnNewProductAppeared(Product product)
        {
            Handler?.Invoke(product);
        }

        public void Start()
        {
            if (_started) return;
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
                    products = ProductMonitoringManager.ScrapeCurrentProductsList(Website, TokenSource.Token);
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
}
