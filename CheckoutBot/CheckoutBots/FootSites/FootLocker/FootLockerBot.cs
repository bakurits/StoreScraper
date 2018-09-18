﻿using System;
using System.Net.Http;
using System.Threading;
using CheckoutBot.Models.Checkout;

namespace CheckoutBot.CheckoutBots.FootSites.FootLocker
{
    public class FootLockerBot : FootSitesBotBase
    {
        public override bool Login(string username, string password, CancellationToken token)
        {

            throw new NotImplementedException();
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public const string ReleasePageUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/21";

        public FootLockerBot() : base("Footlocker", "https://www.footlocker.com",
            ReleasePageUrl)
        {
        }
    }
}