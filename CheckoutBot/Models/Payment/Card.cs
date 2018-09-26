using System;

namespace CheckoutBot.Models.Payment
{
    public class Card : IPaymentMethod
    {
        public string Id { get; set; }
        public string CardHolderName { get; set; }
        public DateTime ValidUntil { get; set; }
        public string CSC { get; set; }
        public PaymentType TypeOfPayment { get; set; }
        public CardType TypeOfCard { get; set; }
    }

    public enum CardType
    {
        MaterCard,
        Visa,
        AmericanExpress,
    }
}
