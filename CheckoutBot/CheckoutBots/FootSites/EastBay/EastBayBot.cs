using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using Jurassic.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl  = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";


        public EastBayBot() : base("EastBay", "http://www.eastbay.com", ApiUrl)
        {

        }

        public override  void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        [JsonObject]
        private class LoginRequestData
        {
            [JsonProperty("action")]
            public string Action { get; set; } = "login";
            [JsonProperty("email")]
            public string Email { get; set; }
            [JsonProperty("password")]
            public string Password { get; set; }
            [JsonProperty("requestKey")]
            public string RequestKey { get; set; }
        }
        public override HttpClient Login(string username, string password)
        {
            HttpClient client = ClientFactory.CreateProxiedHttpClient(autoCookies:true).AddHeaders(ClientFactory.FireFoxHeaders);
//            var client = new HttpClient();
//            client.AddHeaders(("User-Agent",
//                "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0_3 like Mac OS X) AppleWebKit/604.1.34 (KHTML, like Gecko) GSA/37.1.171590344 Mobile/15A432 Safari/604.1"));
            var ignore = client.GetDoc(WebsiteBaseUrl, CancellationToken.None);
            var requestKey = GetRequestKey(client);
            string url = "http://www.eastbay.com/account/gateway";
            LoginRequestData requestData = new LoginRequestData()
            {
                Email = username,
                Password = password,
                RequestKey = requestKey
            };
            var responce = client.PostAsync(url, new StringContent(requestData.JsonToString(), Encoding.UTF8, "application/json")).Result;
            responce.EnsureSuccessStatusCode();
            return client;
        }

        private string GetRequestKey(HttpClient client)
        {
            string res = "";
            client.DefaultRequestHeaders.Clear();

            string url1 = "http://www.eastbay.com/akam/10/781257a1";

            var newHeader = new StringPair[]
            {
                (@"Host", @"www.eastbay.com"),
                (@"User-Agent", @"Mozilla/5.0 (Windows NT 10.0; …) Gecko/20100101 Firefox/61.0"),
                (@"Accept", @"*/*"),
                (@"Accept-Encoding", @"gzip, deflate, br"),
                (@"Accept-Language", @"en-US,en;q=0.5"),
                (@"Referer", @"http://www.eastbay.com/"),
                (@"Cache-Control", @"no-cache"),
                (@"Connection", @"keep-alive"),
                (@"Pragma", @"no-cache"),
            };

            client.AddHeaders(newHeader);

            var resp1 = client.GetAsync(url1);
            resp1.Result.EnsureSuccessStatusCode();

            client.DefaultRequestHeaders.Remove("Accept");
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            client.DefaultRequestHeaders.Add(@"X-Requested-With", @"XMLHttpRequest");

            string url =
                $"http://www.champssports.com/account/gateway?action=requestKey&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            var resp = client.GetAsync(url);
            resp.Result.EnsureSuccessStatusCode();

            //using (var message = new HttpRequestMessage())
            //{
            //    message.Headers.Clear();
            //    message.Headers.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
            //    message.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
            //    message.Headers.Referrer = new Uri("http://www.eastbay.com/");
            //    message.Method = HttpMethod.Get;
               
            //    message.RequestUri = new Uri(url);

            //    var cc = client.SendAsync(message, CancellationToken.None).Result;
            //    cc.EnsureSuccessStatusCode();
            //    res = cc.Content.ReadAsStringAsync().Result;
            //    cc.Dispose();
            //}
            
            var json = Utils.GetFirstJson(res);
            return (string) json["data"]["RequestKey"];
        }


        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName, webSiteBaseUrl, releasePageEndpoint)
        {
        }
    }
}