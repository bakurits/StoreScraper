using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace CheckoutBot.Core
{
    public class ProductMonitoringTask
    {
        /// <summary>
        /// Target Product which to buy immediately after released
        /// </summary>
        public ICheckoutSettings CheckoutInfo { get; set; }

        /// <summary>
        /// Monitoring token source. Monitoring task can be canceled from this tokenSource.
        /// Should be called when user removes task from monitoring list
        /// </summary>
        public CancellationTokenSource MonitoringTokenSource { get; set; }

        public void StartMonitoring()
        {
            Task.Factory.StartNew(
                this.Monitor,
                this.MonitoringTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void Monitor()
        {
            var targetProduct = this.CheckoutInfo.ProductToBuy;
            var buyOptions = this.CheckoutInfo.BuyOptions;

            switch (this.CheckoutInfo)
            {
                case GuestCheckoutSettings guestCheckout:
                {
                    if (!(guestCheckout.ProductToBuy.ScrapedBy is IGuestCheckouter))
                    {
                        throw new InvalidOperationException();
                    }

                    Debug.Assert(targetProduct.ReleaseTime != null, "targetProduct.ReleaseTime != null");
                    var startTime = targetProduct.ReleaseTime.Value - TimeSpan.FromMinutes(3); 
                    Utils.WaitToBecomeTrue(() => DateTime.Now >= startTime, MonitoringTokenSource.Token);

                    break;
                }
                case AccountCheckoutSettings accountCheckout:
                {
                    if (!(accountCheckout.ProductToBuy.ScrapedBy is IAccountCheckouter))
                    {
                        throw new InvalidOperationException();
                    }

                    Debug.Assert(targetProduct.ReleaseTime != null, "targetProduct.ReleaseTime != null");
                    var startTime = targetProduct.ReleaseTime.Value - TimeSpan.FromMinutes(3); 
                    Utils.WaitToBecomeTrue(() => DateTime.Now >= startTime, this.MonitoringTokenSource.Token);
                    break;
                }
            }
        }
    }
}