using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace StoreScraper.Core
{
    public class Logger
    {

        public static Logger Instance = new Logger();

        public event ColoredLogHandler OnLogged;
        
       
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

            OnLogged?.Invoke(log, Color.Red);
        }

    }

    public delegate void ColoredLogHandler(string message, Color color);
}
