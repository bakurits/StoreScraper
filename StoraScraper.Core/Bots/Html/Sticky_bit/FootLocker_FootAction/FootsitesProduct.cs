﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using StoreScraper.Interfaces;
using StoreScraper.Models;

namespace StoreScraper.Bots.Html.Sticky_bit.FootLocker_FootAction
{
    [JsonObject]
    public class FootsitesProduct : Product
    {
        [Browsable(false)]
        public string Sku { get; set; }

        [Browsable(false)]

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Model { get; set; }

        [Browsable(false)]
        public List<string> Sizes { get; set; }

        [Browsable(false)]
        public string Color { get; set; }

        [Browsable(false)]
        public bool LaunchCountdownEnabled { get; set; }

        [Browsable(false)]
        public string Gender { get; set; }

        public FootsitesProduct(IShop scrapedBy, string name, string url, double price, string imageUrl,
            string id, string currency = "$", DateTime? releaseTime = null)
            : base(scrapedBy, name, url, price, imageUrl, id, currency, releaseTime)
        {
        }

        public FootsitesProduct()
        {

        }


        public override string ToString()
        {
            return $"{this.Name}- [{this.Color}]";
        }
    }
}