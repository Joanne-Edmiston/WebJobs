using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    public class LocalQueueListener : IQueueListener
    {
         
        private readonly ITraceWriter _traceWriter;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private readonly int _queuePollIntervalSeconds;

        private const int localMessageVisibilityTimeoutSeconds = 60;
        private const int localMessageProcessBatchsize = 8;

        public LocalQueueListener(
            ITraceWriter traceWriter,
            int queuePollIntervalSeconds = 10)
        {
            if (traceWriter == null)
                throw new ArgumentNullException(nameof(traceWriter));

            _traceWriter = traceWriter;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _queuePollIntervalSeconds = queuePollIntervalSeconds;
        }


        public bool IsStopping
        {
            get
            {
                return _cancellationToken.IsCancellationRequested;
            }

        }



        public void StartListening(string queueName, MethodInfo queueTriggerMethod)
        {

            if (IsStopping)
                throw new InvalidOperationException($"Cannot start listening to queue {queueName} as the listener is stopping");

            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentException($"queueName must not be null or empty", nameof(queueName));

            if (queueTriggerMethod == null)
                throw new ArgumentNullException(nameof(queueTriggerMethod));


            var _listenerTask = Task.Factory.StartNew(() =>
            {
                
                if (_cancellationToken.IsCancellationRequested)
                    throw new InvalidOperationException($"Cannot start listening to queue {queueName} as the listener is stopping");

                _traceWriter.Info($"Start Listening on queue '{queueName}' ");

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
                        messagesInQueue = ProcessQueueNextBatch(queueName, queueTriggerMethod);
                    }


                    if (!_cancellationToken.IsCancellationRequested)
                        Thread.Sleep(_queuePollIntervalSeconds * 1000);

                }

            }, _cancellationToken); 

        }

        public void Stop()
        {
            if (_cancellationTokenSource != null && 
                !_cancellationTokenSource.IsCancellationRequested)
            {
                _traceWriter.Info("Stop listening to queues");
                _cancellationTokenSource.Cancel();
            }
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

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }



        private bool ProcessQueueNextBatch(string queueName, MethodInfo triggerMethod)
        {
            var queueClient = GetQueueClient();
            var queue = queueClient.GetQueueReference(queueName);

            // may not exist if no messages queued yet
            if (!(queue.Exists()))
                return false;

            var visibilityTimeout = new TimeSpan(0, 0, localMessageVisibilityTimeoutSeconds);

            var nextBatch = queue.GetMessages(localMessageProcessBatchsize, visibilityTimeout, null, null);

            if (nextBatch == null || !nextBatch.Any())
                return false;

            foreach (var message in nextBatch)
            {
                _traceWriter.Info($"Processing new message from '{queueName}' ");

                try
                {
                    ProcessMessage(message, triggerMethod);
                }
                catch (Exception ex)
                {
                    _traceWriter.Error($"Failed to process message from queue {queueName}", ex);
                }

                queue.DeleteMessageAsync(message);
            }

            return true;

        }

        private void ProcessMessage(CloudQueueMessage message, MethodInfo triggerMethod)
        {

            if (_cancellationToken.IsCancellationRequested)
                return;

            var paramInfo = triggerMethod.GetQueueTriggerParameter();

            var type = paramInfo.ParameterType;

            var queueObject = JsonConvert.DeserializeObject(message.AsString, type);

            triggerMethod.Invoke(null, new object[] { queueObject });

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
