using System;
using CheckoutBot.Models.Shipping;
using CheckoutBot.Models.Payment;


namespace CheckoutBot.Models
{
    public class Profile
    {
        public string Name { get; set; }
        public ShippinInfo ShippingAddress { get; set; }
        public ShippinInfo BillingAddress { get; set; }
        public Card CreditCard {get; set;}

        public DateTime DateCreated { get; set; }
    }
}
