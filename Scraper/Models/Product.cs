using System.ComponentModel;
using System.Drawing;
using StoreScraper.Factory;
using StoreScraper.Helpers;

namespace StoreScraper.Models
{
    public class Product
    {
        private string v1;
        private string v2;
        private string v3;
        private int v4;
        private int v5;
        private string v6;

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

        public Product(string v1, string v2, string v3, int v4, int v5, string v6)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.v4 = v4;
            this.v5 = v5;
            this.v6 = v6;
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