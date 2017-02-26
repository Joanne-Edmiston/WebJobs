using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

namespace Hawkmoth.Webjobs
{
    internal class LocalTraceWriter :  ITraceWriter
    {
        private readonly TraceLevel _minTraceLevel;

        public LocalTraceWriter(TraceLevel level)
        {
            _minTraceLevel = level;
        }

        public void Error(string message, Exception ex = null)
        {
            if (TraceLevel.Error >= _minTraceLevel)
               DoTrace($"{message}: {ex.ToString()}");
        }

        public void Info(string message)
        {
            if (TraceLevel.Info >= _minTraceLevel)
                DoTrace(message);
        }

        public void Verbose(string message)
        {
            if (TraceLevel.Verbose >= _minTraceLevel)
                DoTrace(message);
        }

        public void Warning(string message)
        {
            if (TraceLevel.Warning >= _minTraceLevel)
                DoTrace(message);
        }


        private void DoTrace(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("s")}:  {message}");
        }
    }
}
