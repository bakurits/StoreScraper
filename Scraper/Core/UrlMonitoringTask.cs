using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    class UrlMonitoringTask : MonitoringTaskBase
    {
        private ScraperBase _scraper;
        private ProductDetails _oldDetails;
        public string Url { get; set; }
        

        public override void MonitorOnce(CancellationToken token)
        {
            var details = _scraper.GetProductDetails(Url, token);

            if (details.SizesList.Count > _oldDetails.SizesList.Count)
            {
               DoFinalActions(details, token);
            }
        }

        public UrlMonitoringTask(string url)
        {
            Uri parsedUri = new Uri(url);

            foreach (var scraper in AppSettings.Default.AvailableScrapers)
            {
                Uri scraperUri = new Uri(scraper.WebsiteBaseUrl);
                var result = Uri.Compare(parsedUri, scraperUri, UriComponents.Host, UriFormat.SafeUnescaped,
                    StringComparison.InvariantCultureIgnoreCase);

                if (result != 0) continue;
                this._scraper = scraper;
                this.Url = url;
                try
                {
                    this._oldDetails = _scraper.GetProductDetails(Url, this.TokenSource.Token);
                }
                catch(Exception e)
                {
                    Logger.Instance.WriteErrorLog($"Error occured while obtaining product info. Please Try Again.. \n msg={e.Message}");
                    throw new Exception("Error occured while obtaining product info");
                }

                return;
            }

            throw new Exception("We don't support this website yet");
        }
    }
}
