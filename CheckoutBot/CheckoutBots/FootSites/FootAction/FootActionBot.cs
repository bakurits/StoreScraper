using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
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

        public HttpClient Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public List<Product> ScrapeReleasePage(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
