using System.Net;
using System.Net.Http;
using Flurl.Http.Configuration;

namespace StoreScraper.Factory
{
    class ProxyHttpClientFactory : DefaultHttpClientFactory
    {
        private string _address;

        public ProxyHttpClientFactory(string address)
        {
            _address = address;
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler
            {
                Proxy = new WebProxy(_address.Replace("\n","")),
                UseProxy = true
            };
        }
    }
}
