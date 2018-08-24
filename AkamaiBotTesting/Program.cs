using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkamaiBypasser
{
  class Program
  {
    static void Main(string[] args)
    {
        var ctx = new WebContext(8090);
        using (var proxy = ctx.Proxy)
        using (var driver = ctx.Driver)
        {
        proxy.Start();

        driver.Url = "http://www.footaction.com";
        driver.Navigate();

        Console.ReadKey();
        }
    }
  }
}
