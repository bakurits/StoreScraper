using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Models;

namespace StoreScraper.Helpers
{
    [JsonObject]
    public class DiscordWebhook
    {

        [JsonProperty("content")] public string Content { get; set; }
        [JsonProperty("username")] public string Username { get; set; }
        [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }

        // ReSharper disable once InconsistentNaming
        [JsonProperty("tts")] public bool IsTTS { get; set; }
        [JsonProperty("embeds")] public List<Embed> Embeds { get; set; } = new List<Embed>();


        public static void PostStartMessage(string apiUrl)
        {
            string myJson = @"{{
                ""content"": ""*reads manual*""
            }}";
            PostMessage(apiUrl, myJson);
        }

        // ReSharper disable once InconsistentNaming
        public static async Task<HttpResponseMessage> Send(string webhookUrl, Product product, string username = null,
            string avatarUrl = null, bool isTTS = false)
        {
            string formatter = @"{{
              ""embeds"": [
                {{
                  ""title"": ""{0}"",
                  ""type"": ""rich"",
                  ""description"": ""{1}"",
                  ""url"": ""{2}"",
                  ""color"": ""7753637"",
                  ""image"": {{
                    ""url"": ""{3}""
                  }},
                  ""timestamp"": ""{4}""
                }}
              ]
            }}";

            string szs;

            try
            {
                szs = product.GetDetails(CancellationToken.None).ToString();
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog($"while getting product details {e.Message}");
                szs = "Error occured while getting details";
            }

            string textMessage = $"*Price*:\\n{product.Price + product.Currency}\\n" +
                                 $"*Store link*:\\n{product.Url}\\n" +
                                 $"*Available sizes are*:\\n{szs}\\n";

            string myJson = string.Format(formatter, product.Name, textMessage, product.Url, 
                product.ImageUrl, DateTime.UtcNow.ToString("O"));
            
            return await PostMessage(webhookUrl, myJson);
            
        }


        private static async Task<HttpResponseMessage> PostMessage(string webhookUrl, string myJson)
        {
            var stringContent = new StringContent(myJson, Encoding.UTF8, "application/json");
            return await ClientFactory.GeneralClient.PostAsync(webhookUrl, stringContent);
        }
    }
}