using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using ScraperCore.Bots.Sticky_bit.EastBay_FootAction;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.FootAction
{
    class FootActionBot : FootAPIBase.FootActionScraper, IGuestCheckouter, IAccountCheckouter, IreleasePageScraper
    {
        public void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public List<Product> ScraperReleasePage(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
