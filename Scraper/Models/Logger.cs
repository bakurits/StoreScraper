using System;
using System.Diagnostics;
using System.Globalization;

namespace StoreScraper.Models
{
    public class Logger
    {
        public enum ProcessingState{ NotStarted, Active, Failed, Success}

        private static readonly Lazy<Logger> Lazy =
            new Lazy<Logger>(() => new Logger());

        public static Logger Instance => Lazy.Value;

        public ProcessingState State;
        public string CurrentLog { get; private set; }
        public event EventHandler<string> OnLogged;

        public void WriteLog(string message)
        {
            OnLogged?.Invoke(this, message);

            string nowTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            CurrentLog += nowTime + " :  " + message + Environment.NewLine + Environment.NewLine;
        }
    }
}
