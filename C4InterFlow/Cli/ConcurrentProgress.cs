using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Serilog;

namespace C4InterFlow.Cli
{
    public class ConcurrentProgress
    {

        private ConcurrentCounter Counter { get; set; }
        private IProgress<int> Progress { get; set; }
        private int ItemsCount { get; set; }
        public ConcurrentProgress(int itemsCount)
        {
            ItemsCount = itemsCount;
            Progress = new Progress<int>(i =>
            {
                Log.Information("Processed {ItemNumber} of {ItemsCount} items", i, ItemsCount);
            });

            Counter = new ConcurrentCounter();
        }

        public void Increment()
        {
            Counter.Increment();

            if (Counter.Count % 10 == 0) // Update every 10 items
            {
                lock (Progress)
                {
                    Progress.Report(Counter.Count);
                }
            }
        }

        public void Complete()
        {
            Progress.Report(Counter.Count);
        }
    }
}
