using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using StoreScraper.Helpers;
using StoreScraper.Interfaces;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Core
{
    public abstract class ScraperBase : IWebsiteScraper
    {
        [Browsable(false)] public abstract string WebsiteName { get; set; }

        /// <summary>
        /// Full url of website http://example.com.
        /// </summary>
        public abstract string WebsiteBaseUrl { get; set; }

        /// <summary>
        /// Template for search settings, such as filters and any other settings.
        /// Object of this type will be provided in FindItems method as settings parameter.
        /// Any bot can use or implement any custom type for this field.
        /// </summary>
        public virtual Type SearchSettingsType { get; set; } = typeof(SearchSettingsBase);


        /// <summary>
        /// Determines bot state. Bot is Active when user have at least 1 product in monitoring list.
        /// </summary>
        public abstract bool Active { get; set; }


        /// <summary>
        /// Searches Products by searchCriteria.
        /// </summary>
        /// <param name="listOfProducts">List in which to add scraped products</param>
        /// <param name="settings">Search settings and buying options.
        /// each bot may implement and use custom type of settings depending on store</param>
        /// <param name="token">Cancellation token to terminate process when cancel requested</param>
        public abstract void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token);


        /// <summary>
        /// Scrapes All products from new arrivals page
        /// </summary>
        /// <param name="listOfProducts">All products that exist on new arrivals page</param>
        /// <param name="token">Cancellation token for canceling task. </param>
        public virtual void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method finds products available sizes
        /// </summary>
        /// <param name="productUrl">Url of product to scrap</param>
        /// <param name="token"></param>
        /// <returns>List of sizes</returns>
        public abstract ProductDetails GetProductDetails(string productUrl, CancellationToken token);


        /// <summary>
        /// Wrapper for FinItems Methods. It does some search settings refactoring before executing FindItems
        /// </summary>
        /// <param name="listOfProducts"></param>
        /// <param name="settings"></param>
        /// <param name="token"></param>
        public void ScrapeItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            List<Product> products = new List<Product>();
            List<Exception> exceptions = new List<Exception>();
           

            switch (settings.Mode)
            {
                case SearchMode.SearchAPI:
                    settings.KeyWords.Split('\n', ',').AsParallel().ForAll(k =>
                    {
                        k = k.Trim();
                        var s = (SearchSettingsBase)settings.Clone();
                        s.KeyWords = k;
                        for (var i = 0; i < AppSettings.Default.ProxyRotationRetryCount; i++)
                        {
                            try
                            {
                                FindItems(out var list, s, token);
                                lock (products)
                                {
                                    products.AddRange(list);
                                }
                                break;
                            }
                            catch (Exception e)
                            {
                                if (i != AppSettings.Default.ProxyRotationRetryCount - 1) continue;
                                Logger.Instance.WriteErrorLog($"Error while search {WebsiteName} with keyword {k}");
                                exceptions.Add(e);
                            }
                        }

                        //if (exceptions.Count > 0) throw new AggregateException(exceptions);
                    });
                    break;
                case SearchMode.NewArrivalsPage:
                {
                    for (var i = 0; i < AppSettings.Default.ProxyRotationRetryCount; i++)
                    {
                        try
                        {
                            ScrapeAllProducts(out var allNewProducts, settings.RequiredScrappingLevel, token);
                            var filteredProducts = allNewProducts.FindAll(p => Utils.SatisfiesCriteria(p, settings));
                            lock (products)
                            {
                                products.AddRange(filteredProducts);
                            }
                            break;
                        }
                        catch (Exception e)
                        {
                            if (i != AppSettings.Default.ProxyRotationRetryCount - 1) continue;
                            Logger.Instance.WriteErrorLog($"Error while new arrivals page search {WebsiteName} with keyword {settings.KeyWords}");
                            exceptions.Add(e);
                        }
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            listOfProducts.AddRange(products);
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