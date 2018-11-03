using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Data;
using StoreScraper.Helpers;
using StoreScraper.Models;
using StoreScraper.Models.Enums;
#pragma warning disable 4014

namespace StoreScraper.Core
{
    public class ProductMonitoringManager
    {
        public delegate void NewProductHandler(Product product);

        [JsonProperty]
        private Dictionary<ScraperBase, ShopMonitoringTask> _allShopTasks { get; set; } = new Dictionary<ScraperBase, ShopMonitoringTask>();



        public void AddNewProductHandler(ScraperBase website, NewProductHandler handler)
        {
            if (_allShopTasks.TryGetValue(website, out var task))
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
            if (_allShopTasks.ContainsKey(website))
            {
                Logger.Instance.WriteVerboseLog("Requested shop monitoring task is already added (Adding Process Skipped)");
                return;
            }

            var curList = await Task.Run(() => ScrapeCurrentProductsList(website, token), token);

            var task = new ShopMonitoringTask()
            {   
                Website = website,
                CurrentProducts = curList,
                TokenSource = new CancellationTokenSource(),
            };
             
            _allShopTasks.Add(website, task);
        }


        public void StartMonitoringTask(ScraperBase website)
        {
            if(!_allShopTasks.TryGetValue(website, out var task)) throw new KeyNotFoundException("Can't start task. Task with this name not found");

            task.Start();
        }

        public void RemoveMonitoringTask(ScraperBase website)
        {
            if (!_allShopTasks.TryGetValue(website, out var task)) throw new KeyNotFoundException($"Can't remove {website} monitoring, because it is not registered");

            task.TokenSource.Cancel();
            _allShopTasks.Remove(website);
        }

        public IReadOnlyCollection<Product> GetCurrentProducts(ScraperBase website)
        {
            if (_allShopTasks.TryGetValue(website, out var task))
            {
                return task.CurrentProducts.ToList().AsReadOnly();
            }

            throw new KeyNotFoundException($"Can't retrieve product list of {website.WebsiteName}, \n because monitoring on this website not runnning");
        }

        public static HashSet<Product> ScrapeCurrentProductsList(ScraperBase website, CancellationToken token)
        {
            List<Product> products = new List<Product>();
            Utils.TrySeveralTimes(() => website.ScrapeAllProducts(out products, ScrappingLevel.Url, token), AppSettings.Default.ProxyRotationRetryCount);


            return new HashSet<Product>(products);
        }

    }
}
