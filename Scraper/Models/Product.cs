using System.ComponentModel;
using System.Drawing;
using StoreScraper.Factory;
using StoreScraper.Helpers;

namespace StoreScraper.Models
{
    public class Product
    {
        [DisplayName("Store")]
        public string StoreName { get; } = "";
        public string Name { get; } = "";

        public double Price { get; set; }

        [Browsable(false)]
        public string Url { get; } = "";

        [Browsable(false)]
        public string Id { get; } = "";

        [Browsable(false)]
        public string ImageUrl { get; set; }

        public Product(string storeName, string name, string url, double price, string id, string imageUrl)
        {
            Name = name.Replace('\n', ' ');
            Url = url;
            Price = price;
            Id = id;
            StoreName = storeName;
            ImageUrl = imageUrl;
        }

        public Product()
        {
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is Product toCompare)) return false;

            return this.Url == toCompare.Url;
        }

        public override int GetHashCode()
        {
            return this.Url.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.Name}-{this.Price}$";
        }
    }
}