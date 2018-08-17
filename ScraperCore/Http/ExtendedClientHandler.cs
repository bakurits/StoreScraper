using System;
using System.Collections.Generic;
using System.IO;
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


            if (nativeMessage.Result.Content.Headers.ContentEncoding.Contains("br"))
            {
                using (var stream = new BrotliStream(nativeMessage.Result.Content.ReadAsStreamAsync().Result, CompressionMode.Decompress))
                {
                    var outputStream = new MemoryStream();
                    stream.CopyTo(outputStream);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    nativeMessage.Result.Content = new StreamContent(outputStream); 
                }
            }


            return nativeMessage;
        }
    }
}
