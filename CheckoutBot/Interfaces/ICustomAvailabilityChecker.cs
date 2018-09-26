using System.Threading;
using CheckoutBot.Models.Checkout;
using StoreScraper.Models;

namespace CheckoutBot.Interfaces
{
    public interface ICustomAvailabilityChecker
    {
        /// <summary>
        /// Checks if specified product is available for purchase with specific options
        /// </summary>
        /// <param name="product">product to check</param>
        /// <param name="token">Canselation token</param>
        /// <param name="options">Method should check if product is purchasable with that buying preferences</param>
        /// <returns></returns>
        bool IsProductAvailable(Product product, CancellationToken token, ProductBuyOptions options);
    }
}