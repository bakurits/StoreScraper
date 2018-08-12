using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Models;

namespace StoreScraper.Helpers
{
    public interface IWebHookPoster
    {
        Task<HttpResponseMessage> PostMessage(string webhookUrl, Product product, CancellationToken token);
    }
}