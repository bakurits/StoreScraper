using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Bakurits.Antonioli;
using StoreScraper.Bots.Html.Bakurits.Kith;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass]
    public class Christianmingle
    {
        private async Task<string> Login()
        {
            var client = ClientFactory.CreateHttpClient(autoCookies: true).AddHeaders(ClientFactory.ChromeHeaders);
            
            var doc = client.GetDoc("https://www.christianmingle.com/en-us", CancellationToken.None);
            Debug.WriteLine(client.DefaultRequestHeaders);
            Debug.WriteLine(doc.DocumentNode.InnerHtml);
            
            return "";
            
            client.AddHeaders(("referer", "https://www.christianmingle.com/en-us/app/login"), ("origin", "https://www.christianmingle.com"));
            
            var values = new Dictionary<string, string>
            {
                { "email", "bakuricucxashvili@gmail.com" },
                { "password", "1234123" }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://www.christianmingle.com/api/auth/v1/login", content);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;

        }
        [TestMethod]
        public void Test()
        {    
            Debug.WriteLine(Login().Result);
            
        }
    }
}