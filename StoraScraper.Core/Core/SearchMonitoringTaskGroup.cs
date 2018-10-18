using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoreScraper.Interfaces;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    public class SearchMonitoringTaskGroup
    {
        public static int CurrId = 1;

        public int Id { get; } = CurrId++;
        public List<ScraperBase> WebsiteList { get; set; }
        public MonitoringOptions Options { get; set; }


        public override string ToString()
        {
            return $"[WebsiteCount: {WebsiteList.Count}] [Filter: {Options.Filter.KeyWords}({Options.Filter.MinPrice}, {Options.Filter.MaxPrice})]";
        }
    }
}
