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
            var childInstance =  (SearchSettingsBase)Activator.CreateInstance(childType);

            childInstance.KeyWords = source.KeyWords;
            childInstance.MaxPrice = source.MaxPrice;
            childInstance.MinPrice = source.MinPrice;
            childInstance.NegKeyWords = source.NegKeyWords;

            return childInstance;
        }


        private string _keywords = "";

        [Category(FilterCatName), DisplayName("Search Text:")]
        public string KeyWords
        {
            get => _keywords;
            set
            {
                _keywords = value;
                ParsedKeywords = value.Split(',', '\n').Select(text => text.Trim().ToLower().Split(' ')).ToArray();
            }
        }

        private string _negKeywords = "";

        [Category(FilterCatName), DisplayName("Negative Keywords")]
        public string NegKeyWords
        {
            get => _negKeywords;
            set
            {
                _negKeywords = value;
                ParsedNegKeywords = value.Split(',', '\n').Select(text => text.Trim().ToLower().Split(' ')).ToArray();
            }
        }

        [Category(FilterCatName), DisplayName("Max. Price")]
        public double MaxPrice { get; set; } = 0;

        [Category(FilterCatName), DisplayName("Min. Price")]
        public double MinPrice { get; set; } = 0;

        [Category(CommonCatName), DisplayName("Search Mode")]
        public SearchMode Mode { get; set; } = SearchMode.SearchAPI;



        /// <summary>
        /// Array of keywords groups, which is also array or keywords.
        /// item is matched this keywords if any of keyword group in outer array matches product.
        /// inner keyword group matches means all of keywords in group in contained in product name
        /// </summary>
        [Browsable(false)] 
        public string[][] ParsedKeywords { get; set; }


        [Browsable(false)]
        public string[][] ParsedNegKeywords { get; set; }

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