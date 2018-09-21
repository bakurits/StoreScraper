using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;

namespace CheckoutBot.Interfaces
{
    public interface ICheckoutSettings
    {
        FootsitesProduct ProductToBuy { get; set; }
        ProductBuyOptions BuyOptions { get; set; }
    }
}