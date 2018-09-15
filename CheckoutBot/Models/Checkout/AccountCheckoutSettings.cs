using StoreScraper.Models;

namespace CheckoutBot.Models.Checkout
{
    public class AccountCheckoutSettings : ICheckoutSettings
    {
        public string UserLogin { get; set; }
        public string UserPassword { get; set; }
        public string UserCcv2 { get; set; }

        public Product ProductToBuy { get; set; }
        public ProductBuyOptions BuyOptions { get; set; }
    }
}