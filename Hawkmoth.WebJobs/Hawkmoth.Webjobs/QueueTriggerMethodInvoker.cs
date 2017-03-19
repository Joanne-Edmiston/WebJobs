using System.Reflection;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Hawkmoth.Webjobs
{
    public class QueueTriggerMethodInvoker : IQueueTriggerMethodInvoker
    {
        /// <summary>
        /// <see cref="IQueueTriggerMethodInvoker.InvokeAsync(CloudQueueMessage, MethodInfo)"/>
        /// </summary>
        public async Task InvokeAsync(CloudQueueMessage message, MethodInfo triggerMethod)
        {
            var paramInfo = triggerMethod.GetQueueTriggerParameter();

            var type = paramInfo.ParameterType;

            var queueObject = JsonConvert.DeserializeObject(message.AsString, type);

            var result = triggerMethod.Invoke(null, new object[] { queueObject });

            if (triggerMethod.IsAsyncMethod())
            {
                await (Task)result;
            }
        }

    }

}
