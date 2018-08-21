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

        /// <summary>
        /// This Method accuire account and any other cookies need to checkout as logged in user.
        /// Not Called explicitly. Called only from accountCheckout
        /// </summary>
        /// <param name="username">username or email of account</param>
        /// <param name="password">passsword of account</param>
        /// <returns>HttpClient which is used to accuire accout session cookies</returns>
        HttpClient Login(string username, string password);
    }
}