using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.EastBay
{
    class EasBayBot : FootSimpleBase.EastBayScraper, IGuestCheckouter, IAccountCheckouter, IreleasePageScraper
    {
        public void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public List<Product> ScrapeReleasePage(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
