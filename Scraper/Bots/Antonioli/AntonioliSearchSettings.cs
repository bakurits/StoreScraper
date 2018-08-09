using System;
using System.ComponentModel;
using StoreScraper.Models;

namespace StoreScraper.Bots.Antonioli
{
    public class AntonioliSearchSettings : SearchSettingsBase
    {
        public enum GenderEnum{Man, Woman, Both}

        public GenderEnum Gender { get; set; }
    }
}