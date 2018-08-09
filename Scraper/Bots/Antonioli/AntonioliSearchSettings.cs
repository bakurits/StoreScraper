using System;
using System.ComponentModel;
using StoreScraper.Models;

namespace StoreScraper.Bots.Antonioli
{
    public class AntonioliSearchSettings : SearchSettingsBase
    {
        public enum GenderEnum{Both, Man, Woman}

        public GenderEnum Gender { get; set; }
    }
}