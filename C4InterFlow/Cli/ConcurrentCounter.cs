using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Cli
{
    public class ConcurrentCounter
    {
        private int _count;
        public int Count => _count;

        public void Increment()
        {
            Interlocked.Increment(ref _count);
        }
    }
}
