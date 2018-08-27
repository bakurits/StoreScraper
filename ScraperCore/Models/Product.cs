using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using StoreScraper.Models;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using HtmlAgilityPack;
using ScraperCore.Interfaces;

namespace StoreScraper.Models
{
    public class Product
    {
        [DisplayName("Store")]
        public string StoreName { get; set; } = "";


        [Browsable(false)]
        public IWebsiteScraper ScrapedBy { get; set; }

        public string Name { get; set; } = "";

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


        /// <summary>
        /// Exact Release Time of product in seconds.
        /// null means already released.
        /// DateTime.MaxValue means exact release time not yet published, but will be published
        /// DateTime.MinValue means release exact release time is not supported on this website.
        /// </summary>
        [Browsable(false)]
        public DateTime? ReleaseTime { get; set; }


        public Product(IWebsiteScraper scrapedBy, string name, string url, double price, string imageUrl, 
            string id, string currency = "$", DateTime? releaseTime = null)
        {
            this.Name = HtmlEntity.DeEntitize(name.Trim());
            this.Url = url;
            this.Price = price;
            this.Id = id;
            this.ScrapedBy = scrapedBy;
            this.StoreName = scrapedBy?.WebsiteName;
            this.ImageUrl = imageUrl;
            this.Currency = currency;
            this.ReleaseTime = releaseTime;
        }

        public Product()
        {
        }


        public virtual ProductDetails GetDetails(CancellationToken token)
        {
            if(!(this.ScrapedBy is ScraperBase scraperBy)) throw new InvalidOperationException("Scraper which scraped this product not supports GetDetails method");

            return scraperBy.GetProductDetails(this.Url, token);
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

        public override string ToString() => this.Name;
    }
}