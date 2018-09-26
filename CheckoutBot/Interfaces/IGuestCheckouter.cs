using System.Threading;
using CheckoutBot.Models.Checkout;

namespace CheckoutBot.Interfaces
{
    public interface IGuestCheckouter
    {  
        /// <summary>
        /// Does Guest checkout. This method is called before 2min of product release
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="token"></param>
        void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token);
    }
}
