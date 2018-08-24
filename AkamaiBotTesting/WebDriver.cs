using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace AkamaiBypasser
{
    public class WebDriver : FirefoxDriver
  {
    public static string UserAgent { get; } = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";

    private static FirefoxOptions GetOptions(WebContext ctx)
    {
      var options = new FirefoxOptions();

      options.AcceptInsecureCertificates = true;
      // options.AddArgument("--headless");
      options.AddArguments($"--useragent={UserAgent}", "-private");
      options.Proxy = new Proxy()
      {
        Kind = ProxyKind.Manual,
        SslProxy = ctx.Proxy.Address,
        HttpProxy = ctx.Proxy.Address,
        IsAutoDetect = false,
      };

      return options;
    }

    public WebDriver(WebContext ctx) : base(GetOptions(ctx))
    {
    }
  }
}
