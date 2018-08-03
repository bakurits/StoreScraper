using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using StoreScraper.Core;
using StoreScraper.Models;

namespace StoreScraper.Helpers
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
        [JsonProperty("embeds")]
        public List<Embed> Embeds { get; set; } = new List<Embed>();

        // ReSharper disable once InconsistentNaming
        public static async void Send(string webhookUrl, Product product, string username = null, string avatarUrl = null, bool isTTS = false)
        {
            string formatter = @"{{
              ""content"": ""Added new item"",
              ""embeds"": [
                {{
                  ""title"": ""{0}"",
                  ""type"": ""rich"",
                  ""description"": ""{1}"",
                  ""url"": ""{2}"",
                  ""color"": ""7753637"",
                  ""image"": {{
                    ""url"": ""{3}""
                  }}
                }}
              ]
            }}";

            string szs;

            try
            {
                szs = string.Join(";  ", product.GetDetails(CancellationToken.None));
            }
            catch(Exception e)
            {
                Logger.Instance.WriteErrorLog($"while getting product details {e.Message}");
                szs = "Error occured while getting details";
            }

            string myJson = string.Format(formatter, product.Name,  product.Name + " added in site available sizes are : " + szs, product.Url, product.ImageUrl);
            var stringContent = new StringContent(myJson, Encoding.UTF8, "application/json");
            Debug.WriteLine(myJson);
            using (var client = new HttpClient())
            {
                await client.PostAsync(webhookUrl, stringContent);
            }
        }
    }
}
