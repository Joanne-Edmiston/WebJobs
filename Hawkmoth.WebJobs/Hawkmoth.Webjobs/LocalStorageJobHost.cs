using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    public class LocalStorageJobHost : IJobHost
    {
        private CancellationTokenSource _stoppingTokenSource;


        private readonly IQueueTriggerIndexer _queueTriggerIndexer;
        private IQueueListener _queueListener;
        private ITraceWriter _tracerWriter;
        private IQueueTriggerMethodInvoker _queueMethodInvoker;

        public LocalStorageJobHost(
            IQueueTriggerIndexer queueTriggerIndexer = null,
            IQueueListener queueListener = null,
            ITraceWriter tracerWriter = null,
            IQueueTriggerMethodInvoker queueMethodInvoker = null
            )
        {
            _stoppingTokenSource = new CancellationTokenSource();

            _tracerWriter = tracerWriter ?? new LocalTraceWriter(System.Diagnostics.TraceLevel.Verbose);
            _queueTriggerIndexer = queueTriggerIndexer ?? new QueueTriggerIndexer(tracerWriter);
            _queueMethodInvoker = queueMethodInvoker ?? new QueueTriggerMethodInvoker();
            _queueListener = queueListener ?? new LocalQueueListener(tracerWriter, queueMethodInvoker);

        }

        /// <summary>
        /// <see cref="IJobHost.Call(MethodInfo)"/>
        /// </summary>
        public void Call(MethodInfo method)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IJobHost.Call(MethodInfo, IDictionary{string, object})"/>
        /// </summary>
        public void Call(MethodInfo method, IDictionary<string, object> arguments)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IJobHost.Call(MethodInfo, object)"/>
        /// </summary>
        public void Call(MethodInfo method, object arguments)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IJobHost.CallAsync(MethodInfo, CancellationToken)"/>
        /// </summary>
        /// <returns></returns>
        public Task CallAsync(MethodInfo method, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IJobHost.CallAsync(MethodInfo, IDictionary{string, object}, CancellationToken)"/>
        /// </summary>
        /// <returns></returns>
        public Task CallAsync(MethodInfo method, IDictionary<string, object> arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IJobHost.CallAsync(MethodInfo, object, CancellationToken)"/>
        /// </summary>
        public Task CallAsync(MethodInfo method, object arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IJobHost.RunAndBlock"/>
        /// </summary>
        public void RunAndBlock()
        {
            Start();
            _stoppingTokenSource.Token.WaitHandle.WaitOne();
            Stop();
           
        }

        /// <summary>
        /// <see cref="IJobHost.Start"/>
        /// </summary>
        public void Start()
        {
            StartAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// <see cref="IJobHost.StartAsync(CancellationToken)"/>
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Run(() =>
            {
                var queueTriggers = _queueTriggerIndexer.GetAllAzureQueueTriggerMethods();

                foreach (var queueTrigger in queueTriggers)
                {
                    var queueName = queueTrigger.Key;
                    var method = queueTrigger.Value;

                    _queueListener.StartListening(queueName, method);
                }
            });
        }

        /// <summary>
        /// <see cref="IJobHost.Stop"/>
        /// </summary>
        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// <see cref="IJobHost.StopAsync"/>
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            await Task.Run(() =>
            {
                if (_queueListener != null && !_queueListener.IsStopping)
                {
                    _queueListener.Stop();
                }

                _stoppingTokenSource.Cancel();
            });

        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                if (_queueListener != null)
                {
                    _queueListener.Dispose();
                    _queueListener = null;
                }

            }
        }

    }
}
