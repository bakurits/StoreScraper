using StoreScraper.Models;

namespace CheckoutBot.Models.Checkout
{
    public interface ICheckoutSettings
    {
        FootsitesProduct ProductToBuy { get; set; }
        ProductBuyOptions BuyOptions { get; set; }
    }
}