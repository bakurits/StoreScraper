using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using StoreScraper.Factory;
using static StoreScraper.Helpers.Utils;

namespace StoreScraper.Http
{
    public class FirefoxHttpClientStorage
    {
        private Dictionary<string ,HttpClient> _proxiedClients = new Dictionary<string, HttpClient>();
        private HttpClient _proxilessClient = ClientFactory.CreateHttpCLient(null, true).AddHeaders(ClientFactory.DefaultHeaders);


        public HttpClient GetHttpClient(WebProxy proxy)
        {
            var uri = proxy.Address.AbsoluteUri;

            if (_proxiedClients.ContainsKey(uri))
            {
                return _proxiedClients[proxy.Address.AbsoluteUri];
            }

            var client = ClientFactory.CreateProxiedHttpClient(proxy, true).AddHeaders(ClientFactory.DefaultHeaders);
            _proxiedClients.Add(uri, client);

            return client;
        }

        public HttpClient GetHttpClient()
        {
            if (AppSettings.Default.UseProxy)
            {
                if (_proxiedClients.Count > 0)
                {
                    _proxiedClients.Values.ToList().GetRandomValue();
                }
            }

            return _proxilessClient;
        }

    }
}
