using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    public static class Utilities
    {
        /// <summary>
        /// Retrieves the parameter of a method decorated by the <see cref="QueueTriggerAttribute"/>
        /// </summary>
        /// <returns></returns>
        public static ParameterInfo GetQueueTriggerParameter(this MethodInfo method)
        {
            return method
               .GetParameters()
               .Where(p => p.GetCustomAttributes(typeof(QueueTriggerAttribute), false).Length == 1)
               .FirstOrDefault();
        }

        /// <summary>
        /// Returns a <see cref="QueueTriggerAttribute"/> on a method parameter
        /// </summary>
        /// <param name="queueTriggerParam"><see cref="ParameterInfo"/></param>
        /// <returns></returns>
        public static QueueTriggerAttribute GetQueueTriggerAttribute(this ParameterInfo queueTriggerParam)
        {
            return (QueueTriggerAttribute)queueTriggerParam.GetCustomAttribute(typeof(QueueTriggerAttribute));
        }

        /// <summary>
        /// Tests if a Method is an async method
        /// </summary>
        /// <param name="method"><see cref="MethodInfo"/> to check</param>
        /// <returns>true if the method is async</returns>
        public static bool IsAsyncMethod(this MethodInfo method)
        {
            if (method.GetCustomAttributes(typeof(AsyncStateMachineAttribute)).Any())
                return true;

            return false;
        }
    }
}
