namespace StoreScraper.Models
{
    class Card
    {
        public string CardHolder;
        public string CardNumber;
        public string Month;
        public string Year;
        public string CVV;

        public Card(string cardHolder, string cardNumber, string month, string year, string cVV)
        {
            CardHolder = cardHolder;
            CardNumber = cardNumber;
            Month = month;
            Year = year;
            CVV = cVV;
        }
    }
}
