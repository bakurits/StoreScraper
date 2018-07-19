using System;
using System.ComponentModel;

namespace StoreScraper.Models
{
    [Serializable]
    public class SearchSettingsBase
    {
        protected const string FilterCatName = "Filters";
        protected const string CommonCatName = "Common";

        [Category(CommonCatName)]
        public bool LoadImages { get; set; } = false;


        [Category(FilterCatName)] public string KeyWords { get; set; } = "";

        [Category(FilterCatName), DisplayName("Negative Keywords")]
        public string NegKeyWrods { get; set; } = "";

        [Category(FilterCatName), DisplayName("Max. Price")]
        public double MaxPrice { get; set; } = 0;

        [Category(FilterCatName), DisplayName("Min. Price")]
        public double MinPrice { get; set; } = 0;


        public override string ToString()
        {
            return $"{KeyWords}[{MinPrice}-{MaxPrice}]";
        }
    }
}
