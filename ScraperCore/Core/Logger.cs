using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using StoreScraper.Helpers;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace StoreScraper.Core
{
    public class Logger
    {

        public static Logger Instance = new Logger();

        public DateTime LastLogSave { get; set; }
        public DateTime LastSnapshotSave { get; set; }
        public event ColoredLogHandler OnLogged;

        public const int MaxLogBytes = 1024 * 1024 * 10;
        public const string SnapshotFolderName = "HtmlSnapshots";
        public const string LogsFolderName = "Logs";
        public const int SnapshotSaveTimeoutSeconds = 5;

        public Logger()
        {
            LastLogSave = DateTime.Now;
            if (!Directory.Exists(SnapshotFolderName))
            {
                Directory.CreateDirectory(SnapshotFolderName);
            }

            if (!Directory.Exists(LogsFolderName))
            {
                Directory.CreateDirectory(LogsFolderName);
            }

            var files = Directory.GetFiles(SnapshotFolderName);

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void WriteErrorLog(string errorMessage)
        {
            string nowTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            string log = $"[{nowTime}]: [Error] {errorMessage}" + Environment.NewLine + Environment.NewLine;

            OnLogged?.Invoke(log, Color.Red);
        }


        public void WriteVerboseLog(string message)
        {
            string nowTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            string log = $"[{nowTime}]: [Verbose] {message}" + Environment.NewLine + Environment.NewLine;
            OnLogged?.Invoke(log, Color.DodgerBlue);
        }


        public void WriteVerboseLog(string message, Color color)
        {
            string nowTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            string log = $"[{nowTime}]: [Verbose] {message}" + Environment.NewLine + Environment.NewLine;
            OnLogged?.Invoke(log, color);
        }


        public void SaveHtmlSnapshop(HtmlDocument document)
        {
            if(document == null) return;
            if(LastSnapshotSave + TimeSpan.FromSeconds(SnapshotSaveTimeoutSeconds) > DateTime.Now) return;

            this.LastSnapshotSave = DateTime.Now;
            string filename = DateTime.Now.ToString(CultureInfo.InvariantCulture).EscapeFileName() + ".html";

            string filePath = Path.Combine(SnapshotFolderName, filename);

            using (var stream = File.Create(filePath))
            {
                using (var writer = new StreamWriter(stream))
                {
                    try
                    {
                        writer.Write(document.DocumentNode.InnerHtml);
                    }
                    catch
                    {
                        //innored
                    }
                }
            }
        }
    }

    public delegate void ColoredLogHandler(string message, Color color);
}
