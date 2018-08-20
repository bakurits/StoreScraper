using System;
using System.Collections.Generic;
using System.Text;

namespace ScraperCore.Interfaces
{
    public interface IWebsiteScraper
    {
        string WebsiteName { get; set; }
        string WebsiteBaseUrl { get; set; }
    }
}
