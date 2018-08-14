using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using StoreScraper.Models;

namespace ScraperCore.Bots.Sticky_bit.EastBay_FootAction
{
    class FootApiSearchSettings : SearchSettingsBase
    {
        public enum GenderEnum {
            Both,
            [Description("200000")] Man,
            [Description("200001")] Woman,
            [Description("200002")] Boy,
            [Description("200003")] Girl
        }
       

        public GenderEnum Gender { get; set; }
    }
}
