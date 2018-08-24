using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Http2;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;
using Titanium.Web.Proxy.EventArguments;

using AkamaiBypasser.Properties;
using OpenQA.Selenium;

namespace AkamaiBypasser
{
    public class WebProxy : ProxyServer
  {
    public WebContext Context { get; }
    public int Port { get; set; }
    
    public string Address { get; }

      public WebProxy(WebContext context, int port, ExternalProxy forwardTo = null) 
      {
          Context = context;
          Port = port;
        


          if (forwardTo != null)
          {
              this.UpStreamHttpsProxy = forwardTo;
              this.UpStreamHttpProxy = forwardTo;
          }

          var endPoint = new ExplicitProxyEndPoint(
            IPAddress.Parse("127.0.0.1"),
            port,
            true
          );

          Address = $"{endPoint.IpAddress}:{endPoint.Port}";

          BeforeResponse += OnResponse;

          AddEndPoint(endPoint);
          CertificateManager.CertificateEngine = CertificateEngine.BouncyCastle;
          CertificateManager.EnsureRootCertificate();
          CertificateManager.TrustRootCertificate(true);
    }

    public new void Start()
    {
        base.Start();
        Console.WriteLine($"Proxy listening on port ${Port}");
    }

    public async Task OnResponse(object sender, SessionEventArgs args)
    {
        var request = args.WebSession.Request;
        var response = args.WebSession.Response;
       
        if (request.OriginalUrl == "/_bm/bd-1-30")

        if (response.StatusCode == 200)
        {
            string body = await args.GetResponseBodyAsString();

            args.SetResponseBodyString
            (
                Resources.anti_akamai +
                Environment.NewLine +
                body
            );
        }
    }
  }
}
