using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    public static class Utilities
    {

        public static ParameterInfo GetQueueTriggerParameter(this MethodInfo method)
        {
            return method
               .GetParameters()
               .Where(p => p.GetCustomAttributes(typeof(QueueTriggerAttribute), false).Length == 1)
               .FirstOrDefault();
        }


        public static QueueTriggerAttribute GetQueueTriggerAttribute(this ParameterInfo queueTriggerParam)
        {
            return (QueueTriggerAttribute)queueTriggerParam.GetCustomAttribute(typeof(QueueTriggerAttribute));
        }
    }
}
