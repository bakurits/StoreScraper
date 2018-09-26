using System.ComponentModel;
using StoreScraper.Models;

namespace StoreScraper.Bots.Html.Higuhigu.Asphaltgold
{
    public class AsphaltgoldSearchSettings : SearchSettingsBase
    {
        public enum ItemTypeEnum { Both, Sneakers, Apparel }

        [DisplayName("Item type")]
        public ItemTypeEnum ItemType { get; set; }
    }
}
