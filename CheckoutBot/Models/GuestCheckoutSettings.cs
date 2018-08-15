using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models.Shipping;
using StoreScraper.Models;

namespace CheckoutBot.Models
{
    public class GuestCheckoutSettings
    {
        public ShippinInfo Shipping;
        public CardInfo CartInfo;
        public Product ProductToBuy;
        public ProductBuyOptions BuyOptions;
    }

}
