﻿using System;
using System.Collections.Generic;
using System.Threading;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.FootSites.FootAction
{
    class FootActionBot : IGuestCheckouter, IAccountCheckouter, IReleasePageScraper
    {
        public void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public List<Product> ScrapeReleasePage(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
