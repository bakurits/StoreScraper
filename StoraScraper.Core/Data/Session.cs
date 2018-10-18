using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StoreScraper.Core;

namespace StoreScraper.Data
{
    public class Session
    {
        public static Session Current { get; set; } = new Session();

        [JsonIgnore]
        [Browsable(false)] 
        public List<ScraperBase> AvailableScrapers;

        public MonitoringTaskManager TaskManager { get; }

    }
}
