using StoreScraper.Models;

namespace CheckoutBot.Models
{
    public class AccountCheckoutSettings
    {
        public Product ProductToBuy;
        public ProductBuyOptions BuyOptions;
        public string UserLogin;
        public string UserPassword;
    }
}