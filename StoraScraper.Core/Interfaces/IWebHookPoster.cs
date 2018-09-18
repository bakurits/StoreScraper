using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Models;

namespace StoreScraper.Interfaces
{
    public interface IWebHookPoster
    {
        Task<HttpResponseMessage> PostMessage(string webhookUrl, ProductDetails product, CancellationToken token);
    }
}