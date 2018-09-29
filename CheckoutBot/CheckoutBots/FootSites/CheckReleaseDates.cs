using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.Models;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot.CheckoutBots.FootSites
{
    public class CheckReleaseDates
    {
        private readonly EastBayBot _eastBayBot;
        public CheckReleaseDates()
        {
            this._eastBayBot = new EastBayBot();
        }

        public void CheckRelease()
        {
            
            List <FootsitesProduct> products = _eastBayBot.ScrapeReleasePage(CancellationToken.None);
            products.Sort((a, b) =>
            {
                Debug.Assert(a.ReleaseTime != null, "a.ReleaseTime != null");
                Debug.Assert(b.ReleaseTime != null, "b.ReleaseTime != null");
                return DateTime.Compare(a.ReleaseTime.Value, b.ReleaseTime.Value);
            });
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
            Console.WriteLine((string) document.DocumentNode.InnerHtml);
        }
    }
}
