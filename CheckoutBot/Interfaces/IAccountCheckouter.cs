using System.Net.Http;
using System.Threading;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;

namespace CheckoutBot.Interfaces
{
    public interface IAccountCheckouter
    {
        /// <summary>
        /// Does Checkout request from specified account. this method is called before 2min of product release
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="token"></param>
        void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token);
    }
}