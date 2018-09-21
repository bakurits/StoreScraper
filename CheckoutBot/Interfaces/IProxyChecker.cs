using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace CheckoutBot.Interfaces
{
    public interface IProxyChecker
    {
        /// <summary>
        /// Chooses best proxies from proxy pool. Proxies which is detected by website must be filtered
        /// <remarks>
        /// returned proxies should be sorter in response time order.
        /// So proxies on which website responds faster should have smaller index in list
        /// </remarks>
        /// </summary>
        /// <param name="proxyPool"></param>
        /// <param name="maxCount">max number of proxies that should be returned</param>
        /// <returns></returns>
        List<WebProxy> ChooseBestProxies(List<WebProxy> proxyPool, int maxCount);
    }
}