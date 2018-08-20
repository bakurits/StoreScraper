namespace CheckoutBot.Models.Payment
{
    public interface IPaymentMethod
    {
        /// <summary>
        /// Identifier of payment method: card number, paypal email, etc...
        /// </summary>
        string Id { get; set; }

        PaymentType TypeOfPayment { get; set; }
    }


    public enum PaymentType
    {
        Card,
        Paypal,
    }
}