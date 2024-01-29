using C4InterFlow.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace C4InterFlow.Cli
{
    public static class CommandExecutionContext
    {
        private static IArchitectureAsCodeReaderContext? _currentArchitectureAsCodeReaderContext;

        public static IArchitectureAsCodeReaderContext CurrentArchitectureAsCodeReaderContext
        {
            get
            {
                if (_currentArchitectureAsCodeReaderContext == null)
                {
                    throw new InvalidOperationException("Architecture As Code Reader Context is not set.");
                }

                return _currentArchitectureAsCodeReaderContext;
            }
        }

        public static void SetCurrentArchitectureAsCodeReaderContext(IArchitectureAsCodeReaderContext context)
        {
            _currentArchitectureAsCodeReaderContext = context;
        }
    }
}
