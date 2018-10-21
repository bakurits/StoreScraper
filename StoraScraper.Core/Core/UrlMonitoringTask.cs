using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StoreScraper.Models;
using StoreScraper.Data;

namespace StoreScraper.Core
{
    public class UrlMonitoringTask : MonitoringTaskBase
    {
        private readonly ScraperBase _scraper;
        private ProductDetails _oldDetails;
        public string Url { get; set; }

        public List<WebHook> WebHooks { get; set; }
        
        public void Start()
        {
            Task.Factory.StartNew(StartMonitoring, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void StartMonitoring()
        {

            Task.Factory.StartNew(() =>
            {
                while (!TokenSource.Token.IsCancellationRequested)
                {
                    ProductDetails details = null;
                    try
                    {
                        details = _scraper.GetProductDetails(Url, TokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.WriteErrorLog($"Error while monitoring url ({Url}) \n msg={e.Message}");
                        continue;
                    }

                    if (details.SizesList.Count > _oldDetails.SizesList.Count)
                    {
                        Logger.Instance.WriteVerboseLog("New Size was found in stock");
                        SendNotification(details, WebHooks);
                    }
                    else
                    {
                        Logger.Instance.WriteVerboseLog("Url monitoring epoch completed. No new sizes in stock");
                    }

                    _oldDetails = details;

                    Task.Delay(AppSettings.Default.MonitoringInterval, TokenSource.Token).Wait(TokenSource.Token);
                }
            }, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public UrlMonitoringTask(string url)
        {
            Uri parsedUri = new Uri(url);

            foreach (var scraper in Session.Current.AvailableScrapers)
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

        public override string ToString()
        {
            return $"UrlMonitoring: {Url}";
        }
    }
}
