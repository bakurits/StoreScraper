using System.Threading;
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
        /// Logins to account with specified credentials
        /// </summary>
        /// <param name="username">Account username</param>
        /// <param name="password">account password</param>
        /// <param name="token"></param>
        /// <returns>True if credentials is correct, otherwise false</returns>
        bool Login(string username, string password, CancellationToken token);
    }
}