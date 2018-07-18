using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CheckOutBot.Helpers
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
