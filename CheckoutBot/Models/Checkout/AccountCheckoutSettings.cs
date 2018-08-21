using StoreScraper.Models;

namespace CheckoutBot.Models.Checkout
{
    public class AccountCheckoutSettings : ICheckoutSettings
    {
        public string UserLogin;
        public string UserPassword;

        public Product ProductToBuy { get; set; }
        public ProductBuyOptions BuyOptions { get; set; }
    }
}