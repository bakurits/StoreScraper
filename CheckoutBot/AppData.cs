using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.ChampsSports;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.CheckoutBots.FootSites.FootAction;
using CheckoutBot.CheckoutBots.FootSites.FootLocker;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot
{
    class AppData
    {     
        public static FootSitesBotBase[] AvailableBots = new FootSitesBotBase[]
        {
            //new FootActionBot(),
            //new FootLockerBot(),
            //new ChampsSportsBot(),
            new EastBayBot(), 
        };

        public static CancellationTokenSource ApplicationGlobalTokenSource { get; set; } = new CancellationTokenSource();

        public static HttpClient CommonFirefoxClient =
            ClientFactory.CreateHttpClient(autoCookies: false).AddHeaders(ClientFactory.FireFoxHeaders);

    }
}
