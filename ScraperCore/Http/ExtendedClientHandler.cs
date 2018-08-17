using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brotli;

namespace ScraperCore.Http
{
    public class ExtendedClientHandler : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var nativeMessage =  base.SendAsync(request, cancellationToken);

            

            var hasEncodingHeader = nativeMessage.Result.Headers.TryGetValues("Content-Encoding", out var result);

            if(!hasEncodingHeader) return nativeMessage;


            if (result.Contains("br"))
            {
                using (var stream = new BrotliStream(nativeMessage.Result.Content.ReadAsStreamAsync().Result, CompressionMode.Decompress))
                {
                    nativeMessage.Result.Content = new StreamContent(stream);
                }
            }


            return nativeMessage;
        }
    }
}
