using StoreScraper.Models;

namespace CheckoutBot.Models.Checkout
{
    public interface ICheckoutSettings
    {
        Product ProductToBuy { get; set; }
        ProductBuyOptions BuyOptions { get; set; }
    }
}