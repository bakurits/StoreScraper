using System;
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


        public static void PostMessage(Product product, string apiUrl)
        {
            string formater = @"{{
                ""attachments"": [
                    {{
                        ""fallback"": ""New item added"",
                        ""title"": ""New item added"",
                        ""title_link"": ""{0}"",
                        ""text"": ""{1}"",
                        ""image_url"": ""{2}"",
                        ""color"": ""#764FA5""
                    }}
                ]
            }}";



            string szs;

            try
            {
                szs = string.Join(";  ", product.GetDetails(CancellationToken.None));
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog($"while getting product details {e.Message}");
                szs = "Error occured while getting details";
            }


            string myJson = String.Format(formater, product.Url, "*" + product.Name + "* added in site\n *available sizes are* : " + szs , product.ImageUrl);

            PostMessageAsync(myJson, apiUrl);
        }

        public static async void PostMessageAsync(string messageJson, string apiUrl)
        {
            using (var client = new HttpClient())
            {   
                await client.PostAsync(apiUrl, new StringContent(messageJson, Encoding.UTF8, "application/json"));
                Debug.WriteLine(messageJson);
                Debug.WriteLine(apiUrl);
            }
        }


    }


}