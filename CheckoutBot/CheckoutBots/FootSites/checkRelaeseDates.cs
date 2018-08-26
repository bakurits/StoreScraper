using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

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
            
            List <Product> products = eastBayBot.ScrapeReleasePage(CancellationToken.None);
            products.Sort((a, b) => DateTime.Compare(a.ReleaseTime.Value,b.ReleaseTime.Value));
            foreach (var product in products)
            {
                Console.WriteLine(product.Url);
            }
        }

        public void checkPost(Product product, CancellationToken token)
        {
            Console.Write(product.Url);
            var request = ClientFactory.CreateProxiedHttpClient().AddHeaders(ClientFactory.ChromeHeaders);
            var document = request.GetDoc(product.Url, token);
            Console.WriteLine(document.DocumentNode.InnerHtml);
        }
    }
}
