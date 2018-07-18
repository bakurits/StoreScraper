using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckOutBot.Models
{
    public sealed class CheckoutTask
    {
        public string StoreName { get; set; }

        public object SearchSettings { get; set; }

        public object BuyOptions { get; set; }

        public CheckoutTask(string storeName, object searchSettings, object buyOptions) =>
            (StoreName, SearchSettings, BuyOptions) = (storeName, searchSettings, buyOptions);


        public override string ToString()
        {
            return $"{StoreName}, {SearchSettings}, {BuyOptions}";
        }
    }
}
