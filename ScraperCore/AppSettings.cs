using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using ScraperCore;
using StoreScraper.Models;


namespace StoreScraper
{
    [Serializable]
    public class AppSettings
    {
        public const string DataFileName = "config.xml";
        public static AppSettings Default;
        public static string DataDir;
        [XmlIgnore] public static string DataFilePath;

        [XmlIgnore] [Browsable(false)] public List<ScraperBase> AvailableScrapers;

        public static int InitialBrowserCount { get; set; } = 0;

        [Browsable(false)]
        public List<string> Proxies { get; set; } = new List<string>();

        public bool UseProxy { get; set; } = false;

        public int ProxyRotationRetryCount { get; set; } = 5;

        [DisplayName("Monitring Delay(ms)")]
        public int MonitoringDelay { get; set; } = 1000;

        public List<WebHook> Webhooks { get; set; } = new List<WebHook>();

        public bool PostStartMessage { get; set; } = false;

        public static void Init()
        {
            DataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Env.ProductName);

            DataFilePath = Path.Combine(DataDir, DataFileName);
        }

        public static AppSettings Load()
        {
            Init();

            if (!File.Exists(DataFilePath))
            {
                return new AppSettings();
            }

            var serializer = new XmlSerializer(typeof(AppSettings));
            var stream = new FileStream(DataFilePath, FileMode.Open);

            try
            {
                var data = serializer.Deserialize(stream) as AppSettings;
                stream.Dispose();
                return data;
            }
            catch
            {
                stream.Dispose();
                File.Delete(DataFilePath);
                return new AppSettings();
            }
        }


        public void Save()
        {
            var serializer = new XmlSerializer(typeof(AppSettings));
            var stream = new FileStream(DataFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            serializer.Serialize(stream, this);

            stream.Dispose();
        }
    }
}