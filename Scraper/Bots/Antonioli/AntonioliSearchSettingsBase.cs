using System;
using StoreScraper.Models;

namespace StoreScraper.Bots.Antonioli
{
    public class AntonioliSearchSettingsBase : SearchSettingsBase
    {
        public enum GenderEnum { Men, Women }
        public GenderEnum Gender { get; set; } = GenderEnum.Men;
    }
}