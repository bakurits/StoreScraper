using System;
using System.ComponentModel;

namespace CheckoutBot.Models.Payment
{
    public class CardInfo
    {
        public string CardHolderName { get; set; }

        public string CartNumber { get; set; }

        public DateTime ValidUntil { get; set; }

        public string CSC { get; set; }
    }
}
