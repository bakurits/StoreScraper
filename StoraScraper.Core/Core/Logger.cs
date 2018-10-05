using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using StoreScraper.Helpers;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace StoreScraper.Core
{
    public class Logger
    {

        public static Logger Instance = new Logger();


        public const int MaxLogMesssages = 10000;
        public const string SnapshotFolderName = "HtmlSnapshots";
        public const string LogsFolderName = "Logs";
        public const int SnapshotSaveTimeoutSeconds = 5;

        
        public DateTime LastLogSave { get; set; }
        public DateTime LastSnapshotSave { get; set; }
        public List<LogEntry> Logs { get; set; } = new List<LogEntry>();
        public event ColoredLogHandler OnLogged;


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
            string nowTime = DateTime.UtcNow.ToString("u",CultureInfo.InvariantCulture);

            string log = $"[{nowTime}]: [Error] {errorMessage}" + Environment.NewLine + Environment.NewLine;

            OnLogged?.Invoke(log, Color.Red);

            InternalLogHander(log, Color.Red);
        }


        public void WriteVerboseLog(string message)
        {
            string nowTime = DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture);

            string log = $"[{nowTime}]: [Verbose] {message}" + Environment.NewLine + Environment.NewLine;
            OnLogged?.Invoke(log, Color.DodgerBlue);

            InternalLogHander(log, Color.DodgerBlue);
        }


        public void WriteVerboseLog(string message, Color color)
        {
            string nowTime = DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture);

            string log = $"[{nowTime}]: [Verbose] {message}" + Environment.NewLine + Environment.NewLine;
            OnLogged?.Invoke(log, color);

            InternalLogHander(log, color);
        }


        public void SaveHtmlSnapshop(HtmlDocument document)
        {
            if(document == null) return;
            if(LastSnapshotSave + TimeSpan.FromSeconds(SnapshotSaveTimeoutSeconds) > DateTime.Now) return;

            this.LastSnapshotSave = DateTime.Now;
            string filename = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).EscapeFileName() + ".html";

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


        private void InternalLogHander(string message, Color color)
        {

            lock (Logs)
            {
                Logs.Add(new LogEntry()
                {
                    Text = message,
                    Color = color,
                });

                if (Logs.Count > MaxLogMesssages)
                {
                    string logText = string.Join("\n", Logs.Select(log => log.Text));
                    string filePath = $"{LogsFolderName}/EventLog {DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture).EscapeFileName()}";
                    using (var stream = File.Create(filePath))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(logText);
                            writer.Flush();
                        }
                    }

                    Logs.Clear();
                } 
            }
        }
    }

    public struct LogEntry
    {
        public string Text;
        public Color Color;
    }

    public delegate void ColoredLogHandler(string message, Color color);
}
