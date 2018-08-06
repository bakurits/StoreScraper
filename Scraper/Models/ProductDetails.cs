using System.Collections.Generic;
using System.Linq;

namespace StoreScraper.Models
{
    public class ProductDetails : Product
    {
        public List<StringPair> SizesList { set; get; } = new List<StringPair>();


        /// <summary>
        /// Adds new size in sizes array
        /// </summary>
        /// <param name="size"></param>
        public void AddSize(string sizeName, string sizeStockInfo)
        {
            SizesList.Add((sizeName, sizeStockInfo));
        }


        public override string ToString()
        {
            return string.Join(", \n",SizesList.Select(sizInfo => $"{sizInfo.Key}[{sizInfo.Value}]"));
        }
    }
}