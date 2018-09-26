using CheckoutBot.Interfaces;
using Newtonsoft.Json;

namespace CheckoutBot.Models.Checkout
{
    [JsonObject]
    public class AccountCheckoutSettings : ICheckoutSettings
    {
        public string UserLogin { get; set; }
        public string UserPassword { get; set; }
        public string UserCcv2 { get; set; }
        public string ProductUrl { get; set; }


        

        [JsonIgnore]
        public FootsitesProduct ProductToBuy { get; set; }

        [JsonIgnore]
        public ProductBuyOptions BuyOptions { get; set; }


        public override string ToString()
        {
            return $@"{UserLogin}\n
                    {UserPassword}\n
                    {ProductToBuy}\n
                    {BuyOptions.Size}
                    ";
        }
    }
}