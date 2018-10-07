using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreScraper.Models
{
    public class MonitoringOptions
    {
        public SearchSettingsBase SearchSettings { get; set; }
        public List<WebHook> WebHooks { get; set; }
    }
}
