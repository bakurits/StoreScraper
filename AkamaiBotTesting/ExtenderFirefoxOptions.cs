using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Titanium.Web.Proxy.Models;

namespace AkamaiBypasser
{
    public class ExtendedFirefoxOptions : FirefoxOptions, ICloneable
    {
        public WebProxy LocalProxy { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public ExtendedFirefoxOptions WithNewLocalProxy()
        {
            var cloned = (ExtendedFirefoxOptions) Clone();

            var oldProxy = cloned.Proxy;

            WebProxy localProxy = null;

            if (oldProxy != null && oldProxy.Kind != ProxyKind.Direct && oldProxy.Kind != ProxyKind.Unspecified)
            {
                var asarray = oldProxy.HttpProxy.Split(':');
                var host = asarray[0];
                var port = int.Parse(asarray[1]);
                var extenrnalProxy = new ExternalProxy()
                {
                    HostName = host,
                    Port = port
                }; 
                localProxy = new WebProxy(forwardTo:extenrnalProxy);
            }
            else
            {
                localProxy = new WebProxy();
            }

           
            localProxy.Start();
            LocalProxy = localProxy;
            cloned.LocalProxy = localProxy;

            var newProxy = new Proxy()
            {
                Kind = ProxyKind.Manual,
                HttpProxy = localProxy.Address,
                SslProxy = localProxy.Address,
            };

            cloned.Proxy = newProxy;

            return cloned;
        }
    }
}
