using System;

namespace Hawkmoth.Webjobs
{
    public interface ITraceWriter
    {

        void Error(string message, Exception ex = null);

        void Info(string message);
        
        void Verbose(string message);
        
        void Warning(string message);

    }
}
