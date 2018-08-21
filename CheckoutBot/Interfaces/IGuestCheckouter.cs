using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using HtmlAgilityPack;
using StoreScraper.Models;

namespace CheckoutBot.Interfaces
{
    interface IGuestCheckouter
    {
        void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token);
    }
}
