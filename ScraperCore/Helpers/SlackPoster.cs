using System;
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



        public async Task<HttpResponseMessage> PostMessage(string apiUrl, ProductDetails product, CancellationToken token)
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
                szs = product.GetDetails(token).ToString();
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog($"while getting product details {e.Message}");
                szs = "Error occured while getting details";
            }

            string textMessage = $"*Price*:\\n{product.Price + product.Currency}\\n" +
                                 $"*Store link*:\\n{product.Url}\\n" +
                                 $"*Available sizes are*:\\n{szs}\\n";

            string myJson = string.Format(formater, product.Url, textMessage, product.ImageUrl, product.Name, DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            return await PostMessageAsync(myJson, apiUrl, token);
        }

        public async Task<HttpResponseMessage> PostMessageAsync(string messageJson, string apiUrl, CancellationToken token)
        {
            return await ClientFactory.GeneralClient.PostAsync(apiUrl, new StringContent(messageJson, Encoding.UTF8, "application/json"), token);
        }


    }


}