using StoreScraper.Models;

namespace StoreScraper.Scrapers.Mrporter
{
    public class MrporterSearchSettings : SearchSettingsBase
    {
        public enum Color {
            Black,
            Blue,
            Brown,
            Gray,
            Green,
            Neutrals,
            Pink,
            Red,
            White,
            Yellow
        }

        public Color ProductColor { get; set; }
    }
}