using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoreScraper;
using StoreScraper.Models;

namespace ScraperTest
{
    public static class Helper
    {
       public static SearchSettingsBase SearchSettings = new SearchSettingsBase
        {
            KeyWords = "blue t-shirt",
            NegKeyWrods = "Woman",
            MinPrice = 200,
            MaxPrice = 1000,
        };

        public static void PrintTestReuslts(List<Product> list)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(String.Join("\n", list));
            Console.ForegroundColor = ConsoleColor.Black;
        }
    }
}
