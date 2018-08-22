using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using HtmlAgilityPack;
using StoreScraper.Models;

namespace CheckoutBot.Interfaces
{
    public interface IGuestCheckouter
    {  
        /// <summary>
        /// Does Guest checkout. This method is called before 2min of product release
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="token"></param>
        void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token);
    }
}
