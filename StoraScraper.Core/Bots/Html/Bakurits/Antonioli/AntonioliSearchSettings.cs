using StoreScraper.Models;

namespace StoreScraper.Bots.Html.Bakurits.Antonioli
{
    public class AntonioliSearchSettings : SearchSettingsBase
    {
        public enum GenderEnum{Both, Man, Woman}

        public GenderEnum Gender { get; set; }
    }
}