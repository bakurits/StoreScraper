using System.ComponentModel;

namespace StoreScraper.Models
{
    class BuyOptionsBase
    {
        protected const string MainCatName = "Purchase Options";

        [Category(MainCatName)] public int Quantity { get; set; } = 1;
    }
}
