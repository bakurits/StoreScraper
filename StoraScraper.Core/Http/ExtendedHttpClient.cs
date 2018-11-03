using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StoreScraper.Http
{
    class ExtendedHttpClient : HttpClient
    {
        public HttpClientHandler Handler { get; set; }

        public ExtendedHttpClient(HttpClientHandler handler) : base(handler)
        {
            Handler = handler;
        }
    }
}
