using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Factory;

namespace StoreScraper.Http
{
    public class RequestSender
    {
        public HttpClient Client { get; set; }
        public HttpClientHandler Handler { get; set; }
    }
}
