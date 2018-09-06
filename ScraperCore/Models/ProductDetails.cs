using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using StoreScraper.Core;

namespace StoreScraper.Models
{
    public class ProductDetails : Product
    {
        public List<StringPair> SizesList { set; get; } = new List<StringPair>();

        /// <summary>
        /// Adds new size in sizes array
        /// </summary>
        public void AddSize(string sizeName, string sizeStockInfo)
        {
            SizesList.Add((sizeName, sizeStockInfo));
        }

        public override string ToString()
        {
            return $"{Name} - {Price}{Currency} - {ScrapedBy}" +  string.Join("; ",SizesList.Select(sizInfo => $"{sizInfo.Key}[{sizInfo.Value}]"));
        }
    }
}