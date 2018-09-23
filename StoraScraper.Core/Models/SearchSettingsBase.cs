using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
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

        [Browsable(false)]
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

        [Browsable(false)]
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
        public string[][] ParsedKeywords { get; private set; }

        /// <summary>
        /// Same as <see cref="ParsedKeywords"/> buf for negative keywords
        /// </summary>
        [Browsable(false)]
        public string[][] ParsedNegKeywords { get; private set; }


        /// <summary>
        /// This property describes what kind of product info is required to scrape
        /// </summary>
        [Browsable(false)]
        public ScrappingLevel RequiredScrappingLevel { get; set; } = ScrappingLevel.PrimaryFields;

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