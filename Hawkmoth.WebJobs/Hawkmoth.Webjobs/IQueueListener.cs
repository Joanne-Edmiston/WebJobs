using System;
using System.Reflection;

namespace Hawkmoth.Webjobs
{
    public interface IQueueListener : IDisposable
    {
        bool IsStopping { get; }

        void StartListening(string queueName, MethodInfo triggerMethod);

        void Stop();

    }
}
