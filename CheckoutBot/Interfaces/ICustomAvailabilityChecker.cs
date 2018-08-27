using System.Collections.Generic;
using System.Threading;
using StoreScraper.Models;

namespace CheckoutBot.Interfaces
{
    public interface ICustomAvailabilityChecker
    {
        /// <summary>
        /// Checks if specified product is available for purchase
        /// </summary>
        /// <param name="product">product to check</param>
        /// <param name="token">Canselation token</param>
        /// <returns></returns>
        bool IsProductAvailable(Product product, CancellationToken token, string specificSize = "Any");
    }
}