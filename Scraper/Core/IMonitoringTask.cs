using System.Threading;

namespace StoreScraper.Core
{
    interface IMonitoringTask
    {
        ScraperBase Bot { get; set; }

        bool Do(CancellationToken token);
    }
}
