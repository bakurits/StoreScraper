﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Interfaces;
using StoreScraper.Models;
#pragma warning disable 4014

namespace StoreScraper.Helpers
{
    public class SlackPoster : IWebHookPoster
    {

        public void PostStartMessage(string apiUrl) =>
            PostMessageAsync(new JObject(new JProperty("text", "Test search started")).ToString(), apiUrl, CancellationToken.None);



        public async Task<HttpResponseMessage> PostMessage(string apiUrl, ProductDetails productDetails, CancellationToken token)
        {
            const string formatter = @"{{
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



            string currency = productDetails.Currency.HtmlDeEntitize();
            string name = productDetails.Name.HtmlDeEntitize().EscapeNewLines();
            string sizes = string.Join("\\n ",
                productDetails.SizesList.Select(sizInfo => $"{sizInfo.Key}[{sizInfo.Value}]".HtmlDeEntitize()));

            string textMessage = $"*Price*:\\n{productDetails.Price + currency}\\n" +
                                 $"*Store link*:\\n{productDetails.Url}\\n" +
                                 $"*Available sizes are*:\\n{sizes}\\n";

            string myJson = string.Format(formatter, productDetails.Url, textMessage, productDetails.ImageUrl, name, DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            return await PostMessageAsync(myJson, apiUrl, token);
        }

        public async Task<HttpResponseMessage> PostMessageAsync(string messageJson, string apiUrl, CancellationToken token)
        {
            var content = new StringContent(messageJson, Encoding.UTF8, "application/json");
            return await ClientFactory.GeneralClient.PostAsync(apiUrl, content, token);
        }


    }


}