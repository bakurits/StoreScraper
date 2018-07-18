using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckOutBot.Interfaces;

namespace CheckOutBot.Core
{
    interface IMonitoringTask
    {
        CheckOutBotBase Bot { get; set; }

        bool Do(CancellationToken token);
    }
}
