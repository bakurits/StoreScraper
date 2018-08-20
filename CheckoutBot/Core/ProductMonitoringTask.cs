using System;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models;
using StoreScraper.Models;

namespace CheckoutBot.Core
{
    public class ProductMonitoringTask
    {
        /// <summary>
        /// Target Product which to buy imediately after released
        /// </summary>
        public ICheckoutSettings CheckoutInfo { get; set; }

        /// <summary>
        /// Monitoring token source. Monitoring task can be canceled from this tokenSource.
        /// Should be called when user removes task from monitoring list
        /// </summary>
        public CancellationTokenSource MonitoringTokenSource { get; set; }

        public void StartMonitoring()
        {
            Task.Factory.StartNew(Monitor, MonitoringTokenSource.Token, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void Monitor()
        {
            throw new NotImplementedException();
        }
    }
}