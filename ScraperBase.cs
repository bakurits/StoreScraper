using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using CheckOutBot.Models;

namespace CheckOutBot.Interfaces
{
    public abstract class CheckOutBotBase
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
        public abstract Type SearchSettings { get; set; }


        /// <summary>
        /// Determinates bot state. Bot is enabled when user have at least 1 product in monitoring list.
        /// </summary>
        public abstract bool Enabled { get; set; }


        /// <summary>
        /// Searches Produts by searchCriteria.
        /// </summary>
        /// <param name="listOfProducts">List in which to add scraped products</param>
        /// <param name="settings">Search settings and buying options.
        /// each bot may implement and use custom type of settings depending on store</param>
        /// <param name="token">Canselation token to terminate process when cancel requested</param>
        /// <param name="info">Object in which method outputs detailed report of finding process</param>
        public abstract void FindItems(out List<Product> listOfProducts, object settings, CancellationToken token,  Logger info);

        public override string ToString()
        {
            return this.WebsiteName;
        }
    }
}