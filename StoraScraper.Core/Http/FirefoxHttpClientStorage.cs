using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using StoreScraper.Http.Factory;
using static StoreScraper.Helpers.Utils;

namespace StoreScraper.Http
{
    public class FirefoxHttpClientStorage
    {
        public Dictionary<string ,HttpClient> ProxiedClients = new Dictionary<string, HttpClient>();
        public HttpClient ProxilessClient = ClientFactory.CreateHttpClient(null, true).AddHeaders(ClientFactory.DefaultHeaders);

  
        public FirefoxHttpClientStorage()
        {
            try
            {
                AppSettings.Default.Proxies.ForEach(proxy =>
                   {
                       var client = ClientFactory.CreateProxiedHttpClient(ClientFactory.ParseProxy(proxy), true).AddHeaders(ClientFactory.DefaultHeaders);
                       ProxiedClients.Add(proxy, client);
                   });
            }
            catch
            {
                //ignored
            }
        }

        public HttpClient GetHttpClient(WebProxy proxy)
        {
            var uri = proxy.Address.AbsoluteUri;

            if (ProxiedClients.ContainsKey(uri))
            {
                return ProxiedClients[proxy.Address.AbsoluteUri];
            }

            var client = ClientFactory.CreateProxiedHttpClient(proxy, true).AddHeaders(ClientFactory.DefaultHeaders);
            ProxiedClients.Add(uri, client);

            return client;
        }

        public HttpClient GetHttpClient()
        {
            if (AppSettings.Default.UseProxy)
            {
                if (ProxiedClients.Count > 0)
                {
                    ProxiedClients.Values.ToList().GetRandomValue();
                }
            }

            return ProxilessClient;
        }

    }
}
