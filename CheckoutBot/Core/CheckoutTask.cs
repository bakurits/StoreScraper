﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Interfaces;
using CheckoutBot.Models.Checkout;
using Newtonsoft.Json;
using StoreScraper.Core;
using StoreScraper.Helpers;

namespace CheckoutBot.Core
{
    [JsonObject]
    public class CheckoutTask
    {
        /// <summary>
        /// Target Product which to buy immediately after released
        /// </summary>
        [Browsable(false)]
        public ICheckoutSettings CheckoutInfo { get; set; }

        [JsonIgnore]
        public string Website => CheckoutInfo.ProductToBuy.ScrapedBy.WebsiteName;

        [JsonIgnore]
        public string ProductName => CheckoutInfo.ProductToBuy.ToString();

        [JsonIgnore]
        public string Quantity => CheckoutInfo.BuyOptions.Quantity.ToString();

        [JsonIgnore]
        public string Type => CheckoutInfo is GuestCheckoutSettings ? "GUEST" : "ACCOUNT";

        public string TotalCost =>
            (CheckoutInfo.ProductToBuy.Price * CheckoutInfo.BuyOptions.Quantity).ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Monitoring token source. Monitoring task can be canceled from this tokenSource.
        /// Should be called when user removes task from monitoring list
        /// </summary>
        [Browsable(false)]
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
                        Utils.WaitToBecomeTrue(() => DateTime.UtcNow >= startTime, MonitoringTokenSource.Token);

                        break;
                    }
                case AccountCheckoutSettings accountCheckout:
                    {
                        if (accountCheckout.ProductToBuy.ScrapedBy is IAccountCheckouter)
                        {
                            Debug.Assert(targetProduct.ReleaseTime != null, "targetProduct.ReleaseTime != null");
                            var startTime = targetProduct.ReleaseTime.Value - TimeSpan.FromMinutes(3);
                            Utils.WaitToBecomeTrue(() => DateTime.UtcNow >= startTime, this.MonitoringTokenSource.Token);
                            try
                            {
                                var checkouterInstance =
                                    (IAccountCheckouter) Activator.CreateInstance(accountCheckout.ProductToBuy.ScrapedBy
                                        .GetType());

                                if (checkouterInstance is IBrowserSession startableBot)
                                {
                                    var proxy = Helper.GetRandomProxy(accountCheckout.ProductToBuy.ScrapedBy);
#if DEBUG
                                    startableBot.Start(proxy:proxy.Address.AbsoluteUri, hidden: true);
#else
                                    startableBot.Start(proxy:proxy.Address.AbsoluteUri, hidden: false);
#endif
                                    
                                }

                                checkouterInstance.AccountCheckout(accountCheckout, MonitoringTokenSource.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                Logger.Instance.WriteVerboseLog("Operation canceled by user", Color.IndianRed);
                            }
                            catch (Exception e) when (!(e is OperationCanceledException))
                            {
                                Logger.Instance.WriteErrorLog(
                                    $"Error occured while checkouting {accountCheckout.ProductToBuy} \n msg={e.Message}");
                            }

                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }

                        break;
                    }
            }
        }
    }
}