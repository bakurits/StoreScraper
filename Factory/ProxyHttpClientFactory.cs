using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http.Configuration;

namespace CheckOutBot.Factory
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
