using System;

namespace CheckoutBot.Models.Payment
{
    public class CartInfo
    {
        public string CardHolderName { get; set; }
        public string CartNumber { get; set; }
        public DateTime ValidUntil { get; set; }
        public string CSC { get; set; }
    }
}
