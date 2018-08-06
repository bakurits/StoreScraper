﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StoreScraper.Core;
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
        public static async Task Send(string webhookUrl, Product product, string username = null,
            string avatarUrl = null, bool isTTS = false)
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
                szs = string.Join(";  ", product.GetDetails(CancellationToken.None).SizesList);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog($"while getting product details {e.Message}");
                szs = "Error occured while getting details";
            }

            string textMessage = $"*{product.Name}* added \\n *Available sizes are*:   {szs} \\n  *Price*:  {product.Price + product.Currency}\\n";

            string myJson = string.Format(formatter, product.Name, textMessage, product.Url, product.ImageUrl);
            
            await PostMessage(webhookUrl, myJson);
            
        }


        private static async Task PostMessage(string webhookUrl, string myJson)
        {
            var stringContent = new StringContent(myJson, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                await client.PostAsync(webhookUrl, stringContent);
            }
        }
    }
}