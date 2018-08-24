using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Models;

namespace AkamaiBypasser
{
  public class WebContext : IDisposable
  {
        public int PortMax = ushort.MaxValue;
        public int PortMin = 30000;


        public WebProxy Proxy { get; set; }
        public WebDriver Driver { get; set; }

        public WebContext(int port)
        {
            Proxy = new WebProxy(this, port);
            Driver = new WebDriver(this);
        }

        public WebContext(ExternalProxy proxy = null)
        {
            int port = GetAvailablePort();

            Proxy = new WebProxy(this, port);
            Driver = new WebDriver(this);
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

      public void Dispose()
      {
          Proxy?.Dispose();
          Driver?.Dispose();
      }
  }
}
