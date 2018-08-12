﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Models;

namespace StoreScraper
{
    public abstract class ScraperBase
    {
        [Browsable(false)] public abstract string WebsiteName { get; set; }

        /// <summary>
        /// Full url of website https://example.com.
        /// </summary>
        public abstract string WebsiteBaseUrl { get; set; }

        /// <summary>
        /// Template for search settings, such as filters and any other settings.
        /// Object of this type will be provided in FindItems method as settings parameter.
        /// Any bot can use or implement any custom type for this field.
        /// </summary>
        public virtual Type SearchSettingsType { get; set; } = typeof(SearchSettingsBase);


        /// <summary>
        /// Determinates bot state. Bot is Active when user have at least 1 product in monitoring list.
        /// </summary>
        public abstract bool Active { get; set; }


        /// <summary>
        /// Searches Produts by searchCriteria.
        /// </summary>
        /// <param name="listOfProducts">List in which to add scraped products</param>
        /// <param name="settings">Search settings and buying options.
        /// each bot may implement and use custom type of settings depending on store</param>
        /// <param name="token">Canselation token to terminate process when cancel requested</param>
        public abstract void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token);

        /// <summary>
        /// This method finds products avaliable sizes
        /// </summary>
        /// <param name="product"></param>
        /// <param name="token"></param>
        /// <returns>List of sizes</returns>
        public abstract ProductDetails GetProductDetails(string productUrl, CancellationToken token);


        /// <summary>
        /// Wrapper for FinItems Methods. It does some search settings refractoring before executing FindItems
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="token"></param>
        public void ScrapeItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            List<Product> products = new List<Product>();
            listOfProducts = products;
            settings.KeyWords.Split(',').AsParallel().ForAll(k =>
            {
                k = k.Trim();
                var s = (SearchSettingsBase)settings.Clone();
                s.KeyWords = k;
                FindItems(out var list, s, token);
                products.AddRange(list);
            });
        }

        public virtual void Initialize()
        {

        }

        public override string ToString()
        {
            return this.WebsiteName ?? "Not Implemented" ;
        }
    }
}