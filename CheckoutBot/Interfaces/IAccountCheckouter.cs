using System.Net.Http;
using System.Threading;
using CheckoutBot.Models;

namespace CheckoutBot.Interfaces
{
    public interface IAccountCheckouter
    {
        void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token);

        /// <summary>
        /// This Method accuire account and any other cookies need to checkout as logged in user.
        /// </summary>
        /// <param name="username">username or email of account</param>
        /// <param name="password">passsword of account</param>
        /// <returns>HttpClient which is used to accuire accout session cookies</returns>
        HttpClient Login(string username, string password);
    }
}