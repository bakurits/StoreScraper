using System;
using System.Threading;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;

namespace CheckoutBot.CheckoutBots.FootSites.ChampsSports
{
    class ChampsSportsBot : IGuestCheckouter, IAccountCheckouter
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
    }
}
