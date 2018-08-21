using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;

namespace CheckoutBot.CheckoutBots.FootSites
{
    public partial class FootSitesBotBase
    {
        /// <summary>
        /// Fills information about shipping and payment in current session.
        /// Will be called immediately obtaining session cookies.
        /// Will be called only in guest checkout.
        /// </summary>
        /// <param name="client">HttpClient in which session cookies is already generated</param>
        /// <param name="info">Shipping information</param>
        /// <param name="payment">Payment information</param>
        /// <param name="token">Token which may be canceled by user anytime</param>
        protected void PostShippingAndPayment(HttpClient client, ShippinInfo info, IPaymentMethod payment, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
