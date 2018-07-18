using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CheckOutBot.Helpers
{
    [JsonObject]
    public class DiscordWebhook
    {

        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        // ReSharper disable once InconsistentNaming
        [JsonProperty("tts")]
        public bool IsTTS { get; set; }


        // ReSharper disable once InconsistentNaming
        public static void Send(string webhookUrl, string content, string username = null, string avatarUrl = null, bool isTTS = false)
        {
            DiscordWebhook hook = new DiscordWebhook()
            {
                Content = content,
                Username = username,
                AvatarUrl = avatarUrl,
                IsTTS = isTTS
            };
            var stringContent = new StringContent(JsonConvert.SerializeObject(hook), Encoding.UTF8, "application/json");

           new HttpClient().PostAsync(webhookUrl, stringContent);
        }
    }
}
