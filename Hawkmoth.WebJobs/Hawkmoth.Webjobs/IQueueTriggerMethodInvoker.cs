using Microsoft.WindowsAzure.Storage.Queue;
using System.Reflection;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    public interface IQueueTriggerMethodInvoker
    {
        /// <summary>
        /// Invokes a queue trigger method asyncronously, with the deserialised
        /// message as the queue trigger parameter
        /// </summary>
        /// <param name="message">Message from Azure Queue</param>
        /// <param name="triggerMethod">Queue Trigger Method to be invoked</param>
        /// <returns><see cref="Task"/></returns>
        Task InvokeAsync(CloudQueueMessage message, MethodInfo triggerMethod);
    }
}
