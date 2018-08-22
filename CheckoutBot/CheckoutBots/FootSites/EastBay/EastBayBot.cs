using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using Jurassic.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl  = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";


        public EastBayBot() : base("EastBay", "http://www.eastbay.com", ApiUrl)
        {

        }

        public override HttpClient Login(string username, string password, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override  void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }



        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName, webSiteBaseUrl, releasePageEndpoint)
        {
        }
    }
}