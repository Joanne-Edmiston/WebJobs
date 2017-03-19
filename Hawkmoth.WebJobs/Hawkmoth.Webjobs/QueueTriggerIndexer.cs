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
    public class QueueTriggerIndexer : IQueueTriggerIndexer
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

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                AddQueueTriggerMethodsInAssembly(assembly, queueTriggerMethods);
            }

            return queueTriggerMethods;
        }


        private void AddQueueTriggerMethodsInAssembly(Assembly assembly, Dictionary<string, MethodInfo> queueMethods)
        {

            try
            {
                var assemblyQueueMethods = assembly
                    .GetTypes()
                    .Where(t => t.IsPublic)
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.GetQueueTriggerParameter() != null && m.IsPublic)
                    .ToArray();

                foreach (var method in assemblyQueueMethods)
                {
                    var queueTriggerParam = method.GetQueueTriggerParameter();

                    if (queueTriggerParam != null)
                    {
                        var queueAttribute = queueTriggerParam.GetQueueTriggerAttribute();

                        if (string.IsNullOrEmpty(queueAttribute.QueueName))
                            throw new InvalidOperationException($"QueueTriggerAttribute must have a QueueName, Method: {method.Name}, Queue Trigger Parameter: {queueTriggerParam.Name}, Assembly: {assembly.GetName().Name}");

                        var queueName = queueAttribute.QueueName.ToLower();

                        if (queueMethods.ContainsKey(queueName))
                            throw new InvalidOperationException($"Cannot have multiple methods triggered by the same queue: {queueAttribute.QueueName}");

                        queueMethods.Add(queueName, method);

                        _traceWriter.Info($"Found queue trigger method {method.Name} for queue {queueName} in assembly {assembly.GetName().Name}");
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Don't treat this as a failure - log error and continue
                _traceWriter.Verbose($"Failed to load types for assembly {assembly.GetName().Name}:");
                foreach (var lEx in ex.LoaderExceptions)
                {
                    _traceWriter.Verbose(lEx.ToString());
                }
            }


        }

      
    }
}
