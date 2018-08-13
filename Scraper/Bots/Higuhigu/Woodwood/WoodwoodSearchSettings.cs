using StoreScraper.Models;

namespace StoreScraper.Bots.Higuhigu.Woodwood
{
    public class WoodwoodSearchSettings : SearchSettingsBase
    {
        public enum GenderEnum { Both, Man, Woman }

        public GenderEnum Gender { get; set; }
    }
}
