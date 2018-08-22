using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models.Checkout;

namespace CheckoutBot.CheckoutBots.FootSites
{
    public partial class FootSitesBotBase
    {
        /// <summary>
        /// Adds product specified in settings to cart.
        /// Will be called imediately when product will get available.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="settings"></param>
        /// <param name="token"></param>
        protected void AddToCart(HttpClient client, ICheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
