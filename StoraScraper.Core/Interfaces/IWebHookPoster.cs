using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Models;

namespace StoreScraper.Interfaces
{
    public interface IWebHookPoster
    {
        /// <summary>
        /// Posts information about new product to webhook.
        /// This method will not throw any exception. Exception handling and logging internally done.
        /// </summary>
        /// <param name="webhookUrl">url to send post request (called webhook)</param>
        /// <param name="product">Product to post information about</param>
        /// <param name="token">Cancellation token to cancel operation anytime</param>
        /// <returns>Response message from server</returns>
        Task<HttpResponseMessage> PostMessage(string webhookUrl, ProductDetails product, CancellationToken token);
    }
}