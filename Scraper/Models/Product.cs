using System.ComponentModel;
using System.Drawing;
using System.Threading;
using StoreScraper.Factory;
using StoreScraper.Helpers;

namespace StoreScraper.Models
{
    public class Product
    {
        [DisplayName("Store")]
        public string StoreName { get; set; } = "";

        [Browsable(false)]
        public ScraperBase ScrapedBy { get; set; }

        public string Name { get; } = "";

        public double Price { get; set; }

        /// <summary>
        /// Short name of currecy.
        /// For example: USD, EUR etc..
        /// </summary>
        public string Currency { get; set; }

        [Browsable(false)]
        public string Url { get; set; } = "";

        [Browsable(false)]
        public string Id { get; set;} = "";

        [Browsable(false)]
        public string ImageUrl { get; set; }

        public Product(ScraperBase scrapedBy, string name, string url, double price, string imageUrl, string id, string currency = "USD")
        {
            this.Name = name.Replace('\n', ' ');
            this.Url = url;
            this.Price = price;
            this.Id = id;
            this.ScrapedBy = scrapedBy;
            this.StoreName = ScrapedBy.WebsiteName;
            this.ImageUrl = imageUrl;
            this.Currency = currency;
        }

        public Product()
        {
        }


        public ProductDetails GetDetails(CancellationToken token)
        {
           return this.ScrapedBy.GetProductDetails(this, token);
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