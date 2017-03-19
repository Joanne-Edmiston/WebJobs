using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    /// <summary>
    /// <see cref="IQueueListener"/>
    /// </summary>
    public class LocalQueueListener : IQueueListener
    {
         
        private readonly ITraceWriter _traceWriter;
        private readonly IQueueTriggerMethodInvoker _queueMethodInvoker;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private readonly int _queuePollIntervalSeconds;
        private readonly int _queuePollBatchSize;
        private readonly int _messageVisiblityTimeoutSeconds;


        public LocalQueueListener(
            ITraceWriter traceWriter,
            IQueueTriggerMethodInvoker queueMethodInvoker,
            int queuePollIntervalSeconds = 10,
            int queuePollBatchSize = 8,
            int messageVisibilityTimeoutSeconds = 60)
        {
            if (traceWriter == null)
                throw new ArgumentNullException(nameof(traceWriter));

            if (queueMethodInvoker == null)
                throw new ArgumentNullException(nameof(queueMethodInvoker));

            _traceWriter = traceWriter;
            _queueMethodInvoker = queueMethodInvoker;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _queuePollIntervalSeconds = queuePollIntervalSeconds;
            _queuePollBatchSize = queuePollBatchSize;
            _messageVisiblityTimeoutSeconds = messageVisibilityTimeoutSeconds;
        }

        /// <summary>
        /// <see cref="IQueueListener.IsStopping"/>
        /// </summary>
        public bool IsStopping
        {
            get
            {
                return _cancellationToken.IsCancellationRequested;
            }

        }


        /// <summary>
        /// <see cref="IQueueListener.StartListening(string, MethodInfo)"/>
        /// </summary>
        public void StartListening(string queueName, MethodInfo queueTriggerMethod)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentException("queueName must not be null or empty");

            if (queueTriggerMethod == null)
                throw new ArgumentNullException(nameof(queueTriggerMethod));


            if (IsStopping)
            {
                _traceWriter.Info($"Cannot start listening to queue {queueName} as the listener is stopping");
                return;
            }

            Task.Factory.StartNew(async () =>
            {
                
                if (_cancellationToken.IsCancellationRequested)
                    throw new InvalidOperationException($"Cannot start listening to queue {queueName} as the listener is stopping");

                _traceWriter.Info($"Start Listening on queue '{queueName}'");

                try
                {
                    bool pollQueue = true;
                    while (pollQueue)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            pollQueue = false;
                            continue;
                        }

                        var messagesInQueue = true;
                        while (messagesInQueue)
                        {
                            messagesInQueue = await ProcessQueueNextBatchAsync(queueName, queueTriggerMethod);
                        }


                        if (!_cancellationToken.IsCancellationRequested)
                            Thread.Sleep(_queuePollIntervalSeconds * 1000);

                    }
                }
                catch (Exception ex)
                {
                    _traceWriter.Error($"Caught an unhandled exception while attempting to process items from queue '{queueName}'", ex);
                }

            }, _cancellationToken); 

        }

        /// <summary>
        /// <see cref="IQueueListener.Stop()"/>
        /// </summary>
        public void Stop()
        {
            if (_cancellationTokenSource != null && 
                !_cancellationTokenSource.IsCancellationRequested)
            {
                _traceWriter.Info("Stop listening to queues");
                _cancellationTokenSource.Cancel();
            }
        }



        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
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

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }



        private async Task<bool> ProcessQueueNextBatchAsync(string queueName, MethodInfo triggerMethod)
        {
           var queueClient = GetQueueClient();
            var queue = queueClient.GetQueueReference(queueName);

            if (!queue.Exists())
            {
                // may not exist if no messages queued yet
                _traceWriter.Verbose($"Queue '{queueName}' does not exist.");
                return false;
            }

            var visibilityTimeout = new TimeSpan(0, 0, _messageVisiblityTimeoutSeconds);

            var nextBatch = await queue.GetMessagesAsync(_queuePollBatchSize, visibilityTimeout, null, null);

            if (nextBatch == null || !nextBatch.Any())
                return false;

            foreach (var message in nextBatch)
            {
                _traceWriter.Info($"Processing new message from '{queueName}'");

                try
                {
                    await ProcessMessageAsync(message, triggerMethod);
                }
                catch (Exception ex)
                {
                    _traceWriter.Error($"Failed to process message from queue {queueName}", ex);
                }

                try
                {
                    await queue.DeleteMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _traceWriter.Error($"Failed to delete message from queue {queueName}", ex);
                }
            }

            return true;

        }

        private async Task ProcessMessageAsync(CloudQueueMessage message, MethodInfo triggerMethod)
        {
            if (_cancellationToken.IsCancellationRequested)
                return;

            await _queueMethodInvoker.InvokeAsync(message, triggerMethod);

        }

        private CloudQueueClient GetQueueClient()
        {
            var storageConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;

            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            var queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry();

            return queueClient;
        }
    }
}
