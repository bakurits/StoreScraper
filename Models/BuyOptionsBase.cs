using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckOutBot.Models
{
    class BuyOptionsBase
    {
        protected const string MainCatName = "Purchase Options";

        [Category(MainCatName)] public int Quantity { get; set; } = 1;
    }
}
