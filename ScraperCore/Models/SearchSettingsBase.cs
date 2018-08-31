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
            var childInstance =  (SearchSettingsBase)Activator.CreateInstance(childType);

            childInstance.KeyWords = source.KeyWords;
            childInstance.MaxPrice = source.MaxPrice;
            childInstance.MinPrice = source.MinPrice;
            childInstance.NegKeyWords = source.NegKeyWords;

            return childInstance;
        }


        [Category(FilterCatName), DisplayName("Search Text:")]
        public string KeyWords { get; set; } = "";

        [Category(FilterCatName), DisplayName("Negative Keywords")]
        public string NegKeyWords { get; set; } = "";

        [Category(FilterCatName), DisplayName("Max. Price")]
        public double MaxPrice { get; set; } = 0;

        [Category(FilterCatName), DisplayName("Min. Price")]
        public double MinPrice { get; set; } = 0;

        public override string ToString()
        {
            return $"{KeyWords}[{MinPrice}-{MaxPrice}]";
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}