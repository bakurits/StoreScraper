using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StoreScraper.Core;

namespace StoreScraper.Data
{
    public class Session
    {

        public const string DataFileName = "Session.json";
        public static string DataFilePath;
        public static Session Current { get; set; } = LoadSession();

        [JsonIgnore]
        [Browsable(false)]
        public List<ScraperBase> AvailableScrapers;

        public MonitoringTaskManager TaskManager { get; set; } = new MonitoringTaskManager();

        
        public ProductMonitoringManager MonitoringManager { get; set; } = new ProductMonitoringManager();


        static Session()
        {
            var dataDir = Path.Combine(
                 Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                 "StoreScraper");

            DataFilePath = Path.Combine(dataDir, DataFileName);
        }

        public static Session LoadSession()
        {
            if (!File.Exists(DataFilePath)) return new Session();

            try
            {
                using (var file = File.OpenText(DataFilePath))
                {
                    var serializer = new JsonSerializer();
                    return (Session)serializer.Deserialize(file, typeof(Session));
                }
            }
            catch
            {
                File.Delete(DataFilePath);
                throw;
            }
        }

        public void SaveSession()
        {
            using (var stream = File.Open(DataFilePath, FileMode.Truncate))
            {
                using (var writer = new StreamWriter(stream))
                {
                    var serializer = new JsonSerializer()
                    {
                        Formatting = Formatting.Indented,
                    };
                    serializer.Serialize(writer, this);
                }
            }
        }

    }
}
