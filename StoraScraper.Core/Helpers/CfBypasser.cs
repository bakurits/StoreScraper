using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Http.Factory;
using StoreScraper.Interfaces;
using static StoreScraper.Http.Factory.ClientFactory;

namespace StoreScraper.Helpers
{
    public static class CfBypasser
    {
        public static HttpResponseMessage GetRequestedPage(WebProxy proxy, IShop shop, string url,  CancellationToken token)
        {
            var initialUrl = new Uri(url);
            var host = new Uri(shop.WebsiteBaseUrl);

            var client = ClientFactory.GetProxiedFirefoxClient();
            string initialPage = null;

            var message1 = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = initialUrl
            };

            message1.Headers.AddHeaders(FireFoxHeaders);

            using (message1)
            {
                var task = client.SendAsync(message1);

                var result = task.Result;

                string pageSource = result.Content.ReadAsStringAsync().Result;
                if (!pageSource.Contains("s,t,o,p,b,r,e,a,k,i,n,g,f"))
                {
                    result.EnsureSuccessStatusCode();
                    return result;
                }

                initialPage = pageSource;
                result.Dispose();
            }

            for (int i = 0; i < 3; i++)
            {
                var engine = new Jurassic.ScriptEngine();
                engine.SetGlobalValue("interop", "15");

                var pass = Regex.Match(initialPage, "name=\"pass\" value=\"(.*?)\"/>").Groups[1].Value;
                var answer = Regex.Match(initialPage, "name=\"jschl_vc\" value=\"(.*?)\"/>").Groups[1].Value;

                var script = Regex.Match(initialPage, "setTimeout\\(function\\(\\){(.*?)}, 4000\\);",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups[1].Value;
                script = script.Replace("a = document.getElementById('jschl-answer');", string.Empty);
                script = script.Replace("f.action += location.hash;", string.Empty);
                script = script.Replace("f.submit();", string.Empty);
                script = script.Replace("f = document.getElementById('challenge-form');", string.Empty);
                script = script.Replace("a.value", "interop");
                script = script.Replace("t = document.createElement('div');", string.Empty);
                script = script.Replace("t.innerHTML=\"<a href='/'>x</a>\";", string.Empty);
                script = script.Replace("t = t.firstChild.href", $"t='{host.AbsoluteUri}';");



                var gga = engine.Evaluate(script);
                var calc = engine.GetGlobalValue<string>("interop");

                Task.Delay(5000, token).Wait(token);
                var uri = new Uri(host, $"/cdn-cgi/l/chk_jschl?jschl_vc={answer}&pass={pass}&jschl_answer={calc}");
                var message3 = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Headers = {Referrer = initialUrl}
                };

                message3.Headers.AddHeaders(FireFoxHeaders);

                using (message3)
                {

                    var finalResult = client.SendAsync(message3, token).Result;


                    string pageSource = finalResult.Content.ReadAsStringAsync().Result;
                    if (!pageSource.Contains("s,t,o,p,b,r,e,a,k,i,n,g,f"))
                    {
                        finalResult.EnsureSuccessStatusCode();
                        return finalResult;
                    }
                    finalResult.Dispose();
                }
            }

            throw new WebException("Couldn't bypass cloudfare");
        }
    }
}
