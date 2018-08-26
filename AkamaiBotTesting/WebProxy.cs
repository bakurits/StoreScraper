using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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

        public int PortMax = ushort.MaxValue;
        public int PortMin = 30000;
        public int Port { get; set; }
    
        public string Address { get; }

        public WebProxy(int port = -1, ExternalProxy forwardTo = null) 
        {
            Port = port;

            if (forwardTo != null)
            {
                this.UpStreamHttpsProxy = forwardTo;
                this.UpStreamHttpProxy = forwardTo;
            }


            

            var endPoint = new ExplicitProxyEndPoint(
            IPAddress.Parse("127.0.0.1"),
            port != -1? port : GetAvailablePort(),
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
            {
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

        private int GetAvailablePort()
        {
            for (int p = PortMin; p < PortMax; p++)
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnections = ipProperties.GetActiveTcpConnections();
                var tcpListeners = ipProperties.GetActiveTcpListeners();

                if(tcpListeners.Any(conn => conn.Port == p) || tcpConnections.Any(conn => conn.LocalEndPoint.Port == p)) continue;

                return p;
            }

            throw new Exception("Available port not found");
        }
  }
}
