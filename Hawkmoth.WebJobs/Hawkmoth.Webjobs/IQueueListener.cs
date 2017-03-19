using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    /// <summary>
    /// Defines operations for handling azure queue messages
    /// </summary>
    public interface IQueueListener : IDisposable
    {
        /// <summary>
        /// Indicates the listener is stopping when true
        /// </summary>
        bool IsStopping { get; }

        /// <summary>
        /// Start processing messages for a specified azure queue
        /// </summary>
        /// <param name="queueName">Name of the queue</param>
        /// <param name="triggerMethod"><see cref="MethodInfo"/> of the static method called to handle the queue messages</param>
        void StartListening(string queueName, MethodInfo triggerMethod);

        /// <summary>
        /// Stop listening to all queues
        /// </summary>
        void Stop();

    }
}
