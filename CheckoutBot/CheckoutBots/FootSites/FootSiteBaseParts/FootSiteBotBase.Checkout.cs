using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CheckoutBot.Models.Checkout;

namespace CheckoutBot.CheckoutBots.FootSites
{
    public partial class FootSitesBotBase
    {
        /// <summary>
        /// Does final checkout post request simulation.
        /// Wll be called immediately when product released, after AddtoCart 
        /// </summary>
        /// <param name="client"></param>
        protected void Checkout(HttpClient client)
        {

        }
    }
}
