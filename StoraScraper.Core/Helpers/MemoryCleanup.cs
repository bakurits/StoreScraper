using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Data;

namespace StoreScraper.Helpers
{
    public static class MemoryCleanup
    {
        static MemoryCleanup()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    GC.Collect();
                    Task.Delay(AppSettings.Default.CleanUpIntervalSec); 
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
