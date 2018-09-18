using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.ChampsSports;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.CheckoutBots.FootSites.FootAction;
using CheckoutBot.CheckoutBots.FootSites.FootLocker;
using Jurassic.Library;
using Newtonsoft.Json;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot
{
    [JsonObject]
    internal class AppData
    {   
        public static AppData Session { get; set; }

        public static FootSitesBotBase[] AvailableBots = new FootSitesBotBase[]
        {
            //new FootActionBot(),
            //new FootLockerBot(),
            //new ChampsSportsBot(),
            new EastBayBot(), 
        };


        

        [JsonIgnore]
        public Dictionary<FootSitesBotBase, List<WebProxy>> ParsedProxies { get; set; }

        [JsonProperty]
        private ProxyGroup[] ProxyGroups { get; set; }

        public CancellationTokenSource ApplicationGlobalTokenSource { get; set; } = new CancellationTokenSource();

        public static HttpClient CommonFirefoxClient =
            ClientFactory.CreateHttpClient(autoCookies: false).AddHeaders(ClientFactory.FireFoxHeaders);


        [JsonObject]
        private class ProxyGroup
        {
            public string SiteName { get; set; }
            public string[] Proxies { get; set; }
        }

    }
}
