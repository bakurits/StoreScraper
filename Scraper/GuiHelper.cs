using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StoreScraper.Core;
using StoreScraper.Models;

namespace StoreScraper
{
    public static class GuiHelper
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.Invoke((MethodInvoker) delegate
            {
                box.SelectionStart = box.TextLength;
                box.SelectionLength = 0;

                box.SelectionColor = color;
                box.AppendText(text);
                box.SelectionColor = box.ForeColor;
            });
        }
        
        public static void LoadPredefinedTasks(this CheckedListBox container, string keywordsSource,
            string negKeywordsSource, string negScrapersSource) 
        {
            if (!File.Exists(keywordsSource)) return;
            
            string[] keywords = File.ReadAllLines(keywordsSource);
            string[] negKeywords =
                File.Exists(negKeywordsSource) ? File.ReadAllLines(negKeywordsSource) : new string[0];
            string[] negScrapers =
                File.Exists(negScrapersSource) ? File.ReadAllLines(negScrapersSource) : new string[0];

            var scraperBases = AppSettings.Default.AvailableScrapers.Where(scraper => Array.Exists(negScrapers, txt => txt == scraper.WebsiteName));

            SearchMonitoringTask monitoringTask = new SearchMonitoringTask()
            {
                Stores = scraperBases.ToList(),
                SearchSettings = new SearchSettingsBase()
                {
                    KeyWords = string.Join(",", keywords),
                    NegKeyWords = string.Join(",", negKeywords)
                }
            };

            container.Items.Add(monitoringTask);
        }
    }
}
