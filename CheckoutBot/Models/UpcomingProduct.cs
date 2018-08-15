using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CheckoutBot.Models
{
    class UpcomingProduct
    {
        /// <summary>
        /// Full name of product
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ReleaseTime of product. DateTime.MaxValue means that release time is not known
        /// </summary>
        public DateTime ReleaseTime { get; set; }

        public string FormattedReleaseTime => ReleaseTime != DateTime.MaxValue ? ReleaseTime.ToString("F") : "Yet Unknown";

        /// <summary>
        /// Image url of product. Used to show product image in bot
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Product Main page url. null if not available yet.
        /// </summary>
        public string Url { get; set; }

        public string FormattedUrl => Url ?? "Yet Unknown";
    }
}
