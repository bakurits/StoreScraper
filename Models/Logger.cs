using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckOutBot.Models
{
    public class Logger
    {
        public enum ProcessingState{ NotStarted, Active, Failed, Success}

        public ProcessingState State;
        public string CurrentLog { get; private set; }
        public event EventHandler<string> OnLogged;

        public void WriteLog(string message)
        {
            OnLogged(this, message);

            CurrentLog += message + Environment.NewLine;
        }
    }
}
