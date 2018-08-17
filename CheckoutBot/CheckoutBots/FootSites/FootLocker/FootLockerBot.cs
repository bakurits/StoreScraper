using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using ScraperCore.Bots.Sticky_bit.EastBay_FootAction;
using StoreScraper;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.FootLocker
{
    class FootLockerBot : FootSitesBotBase
    {
        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public const string ReleasePageUrl = "https://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/21";

        public FootLockerBot() : base("Footlocker", "https://www.footlocker.com", "https://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/21")
        {
        }
    }
}
