using System;
using System.Threading;
using CheckoutBot.Models;

namespace CheckoutBot.CheckoutBots.FootSites.FootLocker
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
