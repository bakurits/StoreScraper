using System;
using System.Collections.Generic;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.Models;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot.CheckoutBots.FootSites
{
    public class CheckRelaeseDates
    {
        private EastBayBot eastBayBot;
        public CheckRelaeseDates()
        {
            this.eastBayBot = new EastBayBot();
        }

        public void checkRelaese()
        {
            
            List <FootsitesProduct> products = eastBayBot.ScrapeReleasePage(CancellationToken.None);
            products.Sort((a, b) => DateTime.Compare(a.ReleaseTime.Value,b.ReleaseTime.Value));
            foreach (var product in products)
            {
                Console.WriteLine(product.Url);
            }
        }

        public void checkPost(FootsitesProduct product, CancellationToken token)
        {
            Console.Write(product.Url);
            var request = ClientFactory.CreateProxiedHttpClient(autoCookies: true).AddHeaders(ClientFactory.FireFoxHeaders);
            var document = request.GetDoc(product.Url, token);
            Console.WriteLine(document.DocumentNode.InnerHtml);
        }
    }
}
