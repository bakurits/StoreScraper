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
    public class GuestCheckoutSettings : ICheckoutSettings
    {
        public ShippinInfo Shipping;
        public Card Cart;

        public Product ProductToBuy{get; set; }
        public ProductBuyOptions BuyOptions{get; set;}
    }

}
