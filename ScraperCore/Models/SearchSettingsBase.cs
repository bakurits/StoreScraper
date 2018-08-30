using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ScraperCore.Models;

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


        private string _keywords;

        [Category(FilterCatName), DisplayName("Search Text:")]
        public string KeyWords
        {
            get => _keywords;
            set
            {
                _keywords = value;
                parsedKeywords = value.Split(',').Select(text => text.Trim().ToLower().Split(' ')).ToArray();
            }
        } 

        [Category(FilterCatName), DisplayName("Negative Keywords")]
        public string NegKeyWrods { get; set; } = "";

        [Category(FilterCatName), DisplayName("Max. Price")]
        public double MaxPrice { get; set; } = 0;

        [Category(FilterCatName), DisplayName("Min. Price")]
        public double MinPrice { get; set; } = 0;

        [Browsable(false)]
        public SearchMode Mode { get; set; } = SearchMode.SearchLink;



        /// <summary>
        /// Array of keywords groups, which is also array or keywords.
        /// item is matched this keywords if any of keyword group in outer array matches product.
        /// inner keyword group matches means all of keywords in group in contained in product name
        /// </summary>
        [Browsable(false)] 
        public string[][] parsedKeywords { get; set; }

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