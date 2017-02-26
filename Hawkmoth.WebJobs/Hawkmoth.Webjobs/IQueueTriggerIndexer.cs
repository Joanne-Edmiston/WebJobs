using System.Collections.Generic;
using System.Reflection;

namespace Hawkmoth.Webjobs
{
    /// <summary>
    /// Defines operations for indexing of Azure Queue Trigger methods
    /// </summary>
    public interface IQueueTriggerIndexer
    {
        /// <summary>
        /// Returns all the Methods with <see cref="Microsoft.Azure.WebJobs.QueueTriggerAttribute"/> decorated params
        /// Within the executing assembly (and all all it's referenced assemblies)
        /// </summary>
        /// <returns><see cref="IDictionary{string, MethodInfo}"/> dictonary of trigger methods, triggered by the name of the queue</returns>
        Dictionary<string, MethodInfo> GetAllAzureQueueTriggerMethods();
    }
}
