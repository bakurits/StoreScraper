﻿using System.ComponentModel;
using StoreScraper.Models;

namespace StoreScraper.Bots.Higuhigu.Asphaltgold
{
    public class AsphaltgoldSearchSettings : SearchSettingsBase
    {
        
        public enum ItemTypeEnum
        {
            Sneakers = 0,
            Apparel = 1
        }

        [DisplayName("Item type")]
        public ItemTypeEnum ItemType { get; set; }
    }
}