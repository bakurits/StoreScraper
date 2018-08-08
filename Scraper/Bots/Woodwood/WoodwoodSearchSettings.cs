using System.ComponentModel;
using StoreScraper.Models;

namespace StoreScraper.Bots.Woodwood
{
    public class WoodwoodSearchSettings : SearchSettingsBase
    {

        public enum GenderEnum
        {
            Male = 0,
            Female = 1
        }

        [DisplayName("Gender")]
        public GenderEnum Gender { get; set; }
    }
}
