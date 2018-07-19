using System.Net.Http;
using System.Text;

namespace StoreScraper.Helpers
{
    class SlackWebHook
    {

        public static async void PostMessageAsync(string message, string apiUrl)
        {
            string myJson = "{'text': '" + message + "'}";
            using (var client = new HttpClient())
            {
               await client.PostAsync(apiUrl, new StringContent(myJson, Encoding.UTF8, "application/json"));
            }
        }

        
    }

   
}
