﻿using System;
using System.ComponentModel;
using System.Threading;
using HtmlAgilityPack;
using ScraperCore.Interfaces;
using StoreScraper.Core;

namespace StoreScraper.Models
{
    public class Product
    {
        [DisplayName("Store")]
        public string StoreName { get; set; } = "";

        [Browsable(false)]
        public string BrandName { get; set; } = "";

        [Browsable(false)]
        public string KeyWords { get; set; } = "";

        [Browsable(false)]
        public IWebsiteScraper ScrapedBy { get; set; }

        public string Name { get; set; } = "";

        [DisplayName("Price")]
        public string PriceStr => Price != 0? Price + Currency : "<unknown price>";

        [Browsable(false)]
        public double Price { get; set; }

        /// <summary>
        /// Short name of currency.
        /// For example: USD, EUR etc..
        /// </summary>
        [Browsable(false)]
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


        public ScrappingLevel ScrappingLevel { get; set; } = ScrappingLevel.PrimaryFields;


        public Product(IWebsiteScraper scrapedBy, string name, string url, double price, string imageUrl,
            string id, string currency = "$", DateTime? releaseTime = null)
        {
            this.Name = name != null ? HtmlEntity.DeEntitize(name.Trim()).Replace("\n", "").Replace("\r", "").Replace("\t", "") : "<unknown name>";
            this.Url = url ?? "<unknown url>";
            this.Price = price;
            this.Id = id;
            this.ScrapedBy = scrapedBy;
            this.StoreName = scrapedBy?.WebsiteName;
            this.ImageUrl = imageUrl;
            this.Currency = currency;
            this.ReleaseTime = releaseTime;
        }


        public Product(IWebsiteScraper scrapedBy, string url)
        {
            this.ScrappingLevel = ScrappingLevel.Url;
            this.Url = url ?? "<unknown url>";
            this.Id = url;
            this.ScrapedBy = scrapedBy;
            this.StoreName = scrapedBy?.WebsiteName;
        }

        public Product(IWebsiteScraper scrapedBy, string url, string name)
        {
            this.ScrappingLevel = ScrappingLevel.NameAndUrl;
            this.Url = url ?? "<unknown url>";
            this.Id = url;
            this.Name = name;
            this.ScrapedBy = scrapedBy;
            this.StoreName = scrapedBy?.WebsiteName;
        }

        public Product()
        {
        }


        /// <summary>
        /// Validates current objects. removes null in any value and logs error in that case.
        /// </summary>
        public void Validate()
        {
            Name = ValidateString(Name, nameof(Name));
            ValidateString(Url, nameof(Url));
            Currency = ValidateString(Currency, nameof(Currency));
            Price = ValidatePrice(Price, nameof(Price));
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

        public static bool operator == (Product p1, Product p2)
        { 
            return p1?.Url == p2?.Url;
        }

        public static bool operator !=(Product p1, Product p2)
        {
            return !(p1 == p2);
        }

        public override int GetHashCode()
        {
            return this.Url.GetHashCode();
        }

        public override string ToString() => this.Name;


        
        private string ValidateString(string value, string variableName)
        {
            if (value == null)
            {
                Logger.Instance.WriteErrorLog($"{StoreName}: Product {variableName} is not scrapped correctly");
                return $"<Unknown {variableName}>";
            }
            else return value;
        }

        private double ValidatePrice(double value, string variableName)
        {
            if (value <= 0 || double.IsNaN(value) || double.IsPositiveInfinity(value))
            {
                Logger.Instance.WriteErrorLog($"{StoreName}: Product {variableName} is not scrapped correctly");
                return 0;
            }
            else return value;
        }
    }
}