using System.Collections.Generic;

namespace StoreScraper.Models
{
    public class ProductDetails : Product
    {
        public List<string> SizesList { set; get; } = new List<string>();


        /// <summary>
        /// Adds new size in sizes array
        /// </summary>
        /// <param name="size"></param>
        public void Add(string size)
        {
            SizesList.Add(size);
        }

    }
}