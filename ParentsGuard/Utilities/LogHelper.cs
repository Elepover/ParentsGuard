using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentsGuard.Utilities
{
    public class LogHelper
    {
        private static readonly string[] LogSources =
        {
            "prng",
            "prng-blocking"
        };
        public static void InitLogging()
        {
            foreach (var source in LogSources)
            {
                if (!EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, "Application");
            }
        }
    }
}
