using StoreScraper.Models;

namespace CheckoutBot.Models
{
    public interface ICheckoutSettings
    {
        Product ProductToBuy { get; set; }
        ProductBuyOptions BuyOptions { get; set; }
    }
}