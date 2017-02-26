using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hawkmoth.Webjobs
{
    /// <summary>
    /// <see cref="IQueueTriggerIndexer"/>
    /// </summary>
    internal class QueueTriggerIndexer : IQueueTriggerIndexer
    {
        private readonly ITraceWriter _traceWriter;


        public QueueTriggerIndexer(ITraceWriter traceWriter)
        {
            if (traceWriter == null)
                throw new ArgumentNullException(nameof(traceWriter));

            _traceWriter = traceWriter;
        }

        /// <summary>
        /// <see cref="IQueueTriggerIndexer.GetAllAzureQueueTriggerMethods"/>
        /// </summary>
        public Dictionary<string, MethodInfo> GetAllAzureQueueTriggerMethods()
        {
            var queueTriggerMethods = new Dictionary<string, MethodInfo>();

            var entryAssmbly = Assembly.GetEntryAssembly();

            GetAllQueueTriggerMethods(entryAssmbly, queueTriggerMethods);

            return queueTriggerMethods;
        }



        private void GetAllQueueTriggerMethods(Assembly assembly, Dictionary<string, MethodInfo> queueTriggeredMethods)
        {
            var entryAssmbly = Assembly.GetEntryAssembly();

            AddQueueTriggerMethodsInAssembly(entryAssmbly, queueTriggeredMethods);

            foreach (var childAssemblyName in entryAssmbly.GetReferencedAssemblies())
            {
                var childAssembly = Assembly.Load(childAssemblyName);

                GetAllQueueTriggerMethods(assembly, queueTriggeredMethods);
            }

        }


        private void AddQueueTriggerMethodsInAssembly(Assembly assembly, Dictionary<string, MethodInfo> queueMethods)
        {

            var assemblyQueueMethods = assembly
                .GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetQueueTriggerParameter() != null)
                .ToArray();

            foreach (var method in assemblyQueueMethods)
            {
                var queueTriggerParam = method.GetQueueTriggerParameter();

                if (queueTriggerParam != null)
                {
                    var queueAttribute = queueTriggerParam.GetQueueTriggerAttribute();

                    if (string.IsNullOrEmpty(queueAttribute.QueueName))
                        throw new InvalidOperationException($"QueueTriggerAttribute must have a QueueName, Method: {method.Name}, Queue Trigger Parameter: {queueTriggerParam.Name}, Assembly: {assembly.FullName}");

                    var queueName = queueAttribute.QueueName.ToLower();

                    if (queueMethods.ContainsKey(queueName))
                        throw new InvalidOperationException($"Cannot have multiple methods triggered by the same queue: {queueAttribute.QueueName}");

                    queueMethods.Add(queueName, method);

                    _traceWriter.Verbose($"Found queue trigger method {method} for queue {queueName} in assembly {assembly.FullName}");
                }
            }

        }

      
    }
}
