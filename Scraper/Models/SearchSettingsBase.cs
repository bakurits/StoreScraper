using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StoreScraper.Models
{
    [Serializable]
    public class SearchSettingsBase : ICloneable
    {
        protected const string FilterCatName = "Filters";
        protected const string CommonCatName = "Common";

        public static SearchSettingsBase ConvertToChild(SearchSettingsBase source, Type childType)
        {
            var childIntance =  (SearchSettingsBase)Activator.CreateInstance(childType);

            childIntance.KeyWords = source.KeyWords;
            childIntance.MaxPrice = source.MaxPrice;
            childIntance.MinPrice = source.MinPrice;
            childIntance.NegKeyWrods = source.NegKeyWrods;

            return childIntance;
        }


        [Category(FilterCatName), DisplayName("Search Text:")]
        public string KeyWords { get; set; } = "";

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

        public object Clone()
        {
            var cloned = (SearchSettingsBase)Activator.CreateInstance(this.GetType());


            cloned.KeyWords = this.KeyWords;
            cloned.NegKeyWrods = this.NegKeyWrods;
            cloned.MinPrice = this.MinPrice;
            cloned.MaxPrice = this.MaxPrice;

            return cloned;
        }
    }
}
