using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.ChampsSports;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.CheckoutBots.FootSites.FootAction;
using CheckoutBot.CheckoutBots.FootSites.FootLocker;

namespace CheckoutBot
{
    class AppData
    {     
        public static FootSitesBotBase[] AvailableBots = new FootSitesBotBase[]
        {
            new FootActionBot(),
            new FootLockerBot(),
            new ChampsSportsBot(),
            new EastBayBot(), 
        };
    }
}
