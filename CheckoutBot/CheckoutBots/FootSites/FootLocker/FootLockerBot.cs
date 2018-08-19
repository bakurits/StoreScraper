using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        public override HttpClient Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public const string ReleasePageUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/21";

        public FootLockerBot() : base("Footlocker", "http://www.footlocker.com", "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/21")
        {
        }
    }
}
