using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.Core;
using CheckoutBot.Models;
using Newtonsoft.Json;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Interfaces;

namespace CheckoutBot
{
    [JsonObject]
    public class AppData
    {
        [JsonIgnore]
        public static AppData Session { get; set; }


        public static string DataFilePath;


        public static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Culture = CultureInfo.InvariantCulture,
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public static string DataDir;

        public const string AppName = "CheckoutBot";

        public static BindingList<FootsitesProduct> CurProductList = new BindingList<FootsitesProduct>();

        public static void Init()
        {  
            DataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppName);

            DataFilePath = Path.Combine(DataDir, "config.json");
        }



        public static AppData Load()
        {
            Init();
            if (!Directory.Exists(DataDir) || !File.Exists(DataFilePath))
            {
                return new AppData();
            }


            var jsonData = File.ReadAllText(DataFilePath);

            try
            {
                var data = JsonConvert.DeserializeObject<AppData>(jsonData);
                return data?? new AppData();
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


        [JsonProperty]
        private List<KeyValuePair<string, List<string>>> _parsedProxies { get; set; } = new List<KeyValuePair<string, List<string>>>();

        public bool UseProxy { get; set; } = false;

        [JsonIgnore]
        public Dictionary<IWebsiteScraper, List<WebProxy>> ParsedProxies { get; set; } =
            new Dictionary<IWebsiteScraper, List<WebProxy>>();


        [JsonIgnore]
        public BindingList<CheckoutTask> CurrentTasks { get; set; } = new BindingList<CheckoutTask>();


        public static CancellationTokenSource ApplicationGlobalTokenSource { get; set; } = new CancellationTokenSource();

        public static HttpClient CommonFirefoxClient =
            ClientFactory.CreateHttpClient(autoCookies: false).AddHeaders(ClientFactory.FireFoxHeaders);



        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            foreach (var parsedProxy in ParsedProxies)
            {
                _parsedProxies.Add(new KeyValuePair<string, List<string>>(parsedProxy.Key.WebsiteName, parsedProxy.Value.Select(proxy => proxy.Address.AbsoluteUri).ToList()));
            }
        }


        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            foreach (var parsedProxy in _parsedProxies)
            {
                ParsedProxies.Add(AvailableBots.Find(bot => bot.WebsiteBaseUrl == parsedProxy.Key), parsedProxy.Value.Select(proxy => new WebProxy(proxy)).ToList());
            }
        }

        public void Save()
        {
            var jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            CreateConfigFileIfNotExists();
            File.WriteAllText(DataFilePath,jsonData);
        }


        private static void CreateConfigFileIfNotExists()
        {
            if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);
            if (!File.Exists(DataFilePath)) File.Create(DataFilePath).Close();
        }

    }
}
