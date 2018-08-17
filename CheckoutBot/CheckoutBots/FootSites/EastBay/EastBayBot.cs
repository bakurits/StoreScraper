﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl  = "https://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";


        public EastBayBot() : base("EastBay", "https://www.eastbay.com", ApiUrl)
        {

        }

        public override  void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override HttpClient Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public EastBayBot(string WebsiteName, string WebSiteBaseUrl, string releasePageEndpoint) : base(WebsiteName, WebSiteBaseUrl, releasePageEndpoint)
        {
        }
    }
}