namespace CheckoutBot.Models.Shipping
{
    public class ShippinInfo
    {
        public AddressTypes AddressType { get; set; }
        public Countries Country { get; set; }
        public States State { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
    }
}
