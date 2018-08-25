using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Titanium.Web.Proxy.Models;

namespace AkamaiBypasser
{
    public class ExtendedFireFoxDriver : FirefoxDriver
    {
        public WebProxy LocalProxy { get; set; }

        public ExtendedFireFoxDriver(ExtendedFirefoxOptions options) : base(options.WithNewLocalProxy())
        {
            LocalProxy = options.LocalProxy;
        }
    }
}
