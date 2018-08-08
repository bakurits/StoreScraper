using System;
using System.ComponentModel;
using StoreScraper.Models;

namespace StoreScraper.Bots.Antonioli
{
    public class AntonioliSearchSettings : SearchSettingsBase
    {
        public enum GenderEnum
        {
            Man = 0,
            Woman = 1
        }
        public GenderEnum Gender { get; set; }
    }
}