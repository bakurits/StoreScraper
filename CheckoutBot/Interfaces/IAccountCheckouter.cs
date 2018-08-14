using System.Threading;
using CheckoutBot.Models;

namespace CheckoutBot.Interfaces
{
    public interface IAccountCheckouter
    {
        void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token);
    }
}