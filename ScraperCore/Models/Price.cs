using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
