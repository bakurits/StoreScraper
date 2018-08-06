﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using StoreScraper.Core;
using StoreScraper.Models;

namespace StoreScraper.Helpers
{
    public class SlackWebHook
    {

        public static void PostStartMessage(string apiUrl)
        {
#pragma warning disable 4014
            PostMessageAsync(@"{
                ""text"": ""Test search started""  
            }", apiUrl);
#pragma warning restore 4014
        }

        public static async Task PostMessage(Product product, string apiUrl)
        {
            const string formater = @"{{
                ""attachments"": [
                    {{
                        ""fallback"": ""{3}"",
                        ""title"": ""{3}"",
                        ""title_link"": ""{0}"",
                        ""text"": ""{1}"",
                        ""thumb_url"": ""{2}"",
                        ""color"": ""#764FA5"",
                        ""ts"": ""{4}""
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

            string myJson = string.Format(formater, product.Url, textMessage, product.ImageUrl, product.Name, DateTime.UtcNow.Subtract(DateTime.MinValue.AddYears(1969)).TotalSeconds);

            await PostMessageAsync(myJson, apiUrl);
        }

        public static async Task PostMessageAsync(string messageJson, string apiUrl)
        {
            using (var client = new HttpClient())
            {   
                await client.PostAsync(apiUrl, new StringContent(messageJson, Encoding.UTF8, "application/json"));
            }
        }


    }


}