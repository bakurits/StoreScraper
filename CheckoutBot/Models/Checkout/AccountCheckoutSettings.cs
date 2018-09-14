using StoreScraper.Models;

namespace CheckoutBot.Models.Checkout
{
    public class AccountCheckoutSettings : ICheckoutSettings
    {
        public readonly string UserLogin;
        public readonly string UserPassword;
        public readonly string UserCvv;

        public AccountCheckoutSettings(string userLogin, string userPassword, string userCvv)
        {
            UserLogin = userLogin;
            UserPassword = userPassword;
            UserCvv = userCvv;
        }

        public Product ProductToBuy { get; set; }
        public ProductBuyOptions BuyOptions { get; set; }
    }
}