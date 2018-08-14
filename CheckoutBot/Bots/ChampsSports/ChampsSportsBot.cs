using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker;

namespace CheckoutBot.Bots.ChampsSports
{
    class ChampsSportsBot : FootSimpleBase.ChampsSportsScraper, IGuestCheckouter, IAccountCheckouter
    {
        public void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
