using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using StoreScraper.Helpers;

namespace StoreScraper.Core
{
    public class Logger
    {

        public static Logger Instance = new Logger();

        public event ColoredLogHandler OnLogged;

        private const string SnapshotFolderName = "HtmlSnapshots";

        public Logger()
        {
            if (!Directory.Exists(SnapshotFolderName))
            {
                Directory.CreateDirectory(SnapshotFolderName);
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

            OnLogged?.Invoke(log, Color.Blue);
        }


        public void SaveHtmlSnapshop(HtmlDocument document)
        {
            if(document == null) return;
            string filename = DateTime.Now.ToString(CultureInfo.InvariantCulture).EscapeFileName() + "html";

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
