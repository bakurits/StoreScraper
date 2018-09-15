using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;
using StoreScraper.Models;

namespace CheckoutBot.Models.Checkout
{
    public class GuestCheckoutSettings : ICheckoutSettings
    {
        public ShippinInfo Shipping;
        public Card Card;

        public FootsitesProduct ProductToBuy{get; set; }
        public ProductBuyOptions BuyOptions{get; set;}
    }

}
