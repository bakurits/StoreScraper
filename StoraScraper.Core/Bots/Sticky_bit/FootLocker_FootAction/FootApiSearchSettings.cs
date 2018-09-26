using System.ComponentModel;
using StoreScraper.Models;

namespace FootLocker_FootAction
{
    public class FootApiSearchSettings : SearchSettingsBase
    {
        public enum GenderEnum {
            Any,
            [Description("200000")] Man,
            [Description("200001")] Woman,
            [Description("200002")] Boy,
            [Description("200003")] Girl
        }
       

        
        public GenderEnum Gender { get; set; }
    }
}
