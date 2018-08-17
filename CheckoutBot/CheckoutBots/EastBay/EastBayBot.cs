using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.EastBay
{
    public class EastBayBot : FootSimpleBase.EastBayScraper, IGuestCheckouter, IAccountCheckouter, IReleasePageScraper
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
            var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            int monthCount = 2; // this variable stores how many month's information needed 
            string url =
                $"https://video.eastbay.com/feeds/release_watch.cfm?variable=products&months={monthCount}&cd=1m&_={timestamp}";
            
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var doc = client.GetDoc(url, token);
            

            throw new NotImplementedException();
        }
    }
}
