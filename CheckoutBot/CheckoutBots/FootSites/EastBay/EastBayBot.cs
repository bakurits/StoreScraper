using System;
using System.Threading;
using CheckoutBot.Models;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
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

        public override void Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName, webSiteBaseUrl, releasePageEndpoint)
        {
        }
    }
}