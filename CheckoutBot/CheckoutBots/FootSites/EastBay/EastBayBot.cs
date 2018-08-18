using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using CheckoutBot.Models;
using Jurassic.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl  = "https://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";


        public EastBayBot() : base("EastBay", "https://www.eastbay.com", ApiUrl)
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
            HttpClient client = ClientFactory.CreateHttpCLient(autoCookies:true).AddHeaders(ClientFactory.FireFoxHeaders);
            var ignore = client.GetDoc(WebsiteBaseUrl, CancellationToken.None);
            var requestKey = GetRequestKey(client);
            string url = "https://www.eastbay.com/account/gateway";
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
            string url =
                $"https://www.eastbay.com/account/gateway?action=requestKey&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            string res = "";
            using (var message = new HttpRequestMessage())
            {
                message.Headers.Clear();
                message.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                message.Headers.TryAddWithoutValidation("X-requested-with", "XMLHttpRequest");
                message.Headers.Referrer = new Uri("https://www.eastbay.com/");
                message.Method = HttpMethod.Get;
                message.RequestUri = new Uri(url);

                //client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.off---white.com/en/US/");
                //client.DefaultRequestHeaders.Remove("Accept");
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");

                var cc = client.SendAsync(message, CancellationToken.None).Result;
                res = cc.Content.ReadAsStringAsync().Result;
                cc.Dispose();
            }
            
            var json = Utils.GetFirstJson(res);
            return (string) json["data"]["RequestKey"];
        }

        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName, webSiteBaseUrl, releasePageEndpoint)
        {
        }
    }
}