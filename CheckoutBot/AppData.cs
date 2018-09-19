using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using Newtonsoft.Json;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot
{
    [JsonObject]
    internal class AppData
    {
        public static AppData Session { get; set; } = AppData.Load();

        public static string DataFilePath { get; set; }

        public static string DataDir { get; set; }

        public const string AppName = "CheckoutBot";

        public static void Init()
        {  
            DataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CheckoutBot");

            DataFilePath = Path.Combine(AppName, "config.json");
        }



        public static AppData Load()
        {
            Init();

            if (!File.Exists(DataFilePath))
            {
                return new AppData();
            }


            var jsonData = File.ReadAllText(DataFilePath);

            try
            {
                var data = JsonConvert.DeserializeObject<AppData>(jsonData);
                return data;
            }
            catch
            {
                //ignored
            } 
           

            try
            {
                File.Delete(DataFilePath);
            }
            catch
            {
                //ignored
            }
            return new AppData();
        }

        public static List<FootSitesBotBase> AvailableBots = new List<FootSitesBotBase>()
        {
            //new FootActionBot(),
            //new FootLockerBot(),
            //new ChampsSportsBot(),
            new EastBayBot(), 
        };


        [JsonIgnore]
        public Dictionary<FootSitesBotBase, List<WebProxy>> ParsedProxies { get; set; }

        [JsonProperty]
        public ProxyGroup[] ProxyGroups
        {
            set
            {
                ParsedProxies =
                    value.ToDictionary(group => AvailableBots.Find(bot => bot.WebsiteName == group.SiteName),
                        group => group.Proxies.Select(proxy => new WebProxy(proxy)).ToList());
            }
        }

        public static CancellationTokenSource ApplicationGlobalTokenSource { get; set; } = new CancellationTokenSource();

        public static HttpClient CommonFirefoxClient =
            ClientFactory.CreateHttpClient(autoCookies: false).AddHeaders(ClientFactory.FireFoxHeaders);



        public void Save()
        {
            var jsonData = JsonConvert.SerializeObject(this);
            File.WriteAllText(DataFilePath,jsonData);
        }


        [JsonObject]
        internal class ProxyGroup
        {
            public string SiteName { get; set; }
            public string[] Proxies { get; set; }
        }

    }
}
