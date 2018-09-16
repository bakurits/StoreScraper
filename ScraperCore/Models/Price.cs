namespace StoreScraper.Models
{
    public struct Price
    {
        public double Value;
        public string Currency;

        public Price(double value, string currency)
        {
            Value = value;
            Currency = currency;
        }

        public override string ToString()
        {
            return $"Value={Value}  Currency={Currency}";
        }
    }
}
