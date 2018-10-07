using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Helpers;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Core
{
    public class SearchMonitoringTask : MonitoringTaskBase
    {
        
        public ScraperBase Store { get; set; } 
        public List<MonitoringOptions> MonitoringOptions { get; set; }

        public override void StartMonitoring()
        {
           
        }
    }
}
