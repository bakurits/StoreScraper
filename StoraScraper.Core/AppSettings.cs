using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using StoreScraper.Core;
using StoreScraper.Models;


namespace StoreScraper
{
    [JsonObject]
    [Serializable]
    public class AppSettings
    {
        public const string DataFileName = "config.json";
        public static AppSettings Default;
        public static string DataDir;

        [JsonIgnore]
        public static string DataFilePath;


        [JsonIgnore]
        [Browsable(false)] 
        public List<ScraperBase> AvailableScrapers;

        [JsonIgnore]
        public Dictionary<ScraperBase, SearchMonitoringTask> MonTasks { get; set; } = new Dictionary<ScraperBase, SearchMonitoringTask>();


        [Browsable(false)]
        public List<string> Proxies { get; set; } = new List<string>();

        public bool UseProxy { get; set; } = false;

        public int ProxyRotationRetryCount { get; set; } = 5;

        [DisplayName("Monitring Interval(ms)")]
        public int MonitoringInterval { get; set; } = 1000;

        public List<WebHook> Webhooks { get; set; } = new List<WebHook>();

        public bool UseGUILogging { get; set; } = true;

        public bool PostStartMessage { get; set; } = false;

        public static void Init()
        {
            DataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "StoreScraper");

            DataFilePath = Path.Combine(DataDir, DataFileName);
        }

        public static AppSettings Load()
        {
            Init();

            if (!File.Exists(DataFilePath))
            {
                return new AppSettings();
            }


            var jsonData = File.ReadAllText(DataFilePath);

            try
            {
                var data = JsonConvert.DeserializeObject<AppSettings>(jsonData);
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
                //ingored
            }
            return new AppSettings();
        }


        public void Save()
        {
            var jsonData = JsonConvert.SerializeObject(this);
            File.WriteAllText(DataFilePath,jsonData);
        }
    }
}