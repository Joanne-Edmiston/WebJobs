using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hawkmoth.WebJobs.Test
{
    public static class QueueUtilities
    {

        public static async Task QueueMessageDataAsync(string queueName, object messageData)
        {
            var queueMessage = CreateQueueMessage(messageData);
            var queue = GetQueueClient().GetQueueReference(queueName);
            queue.CreateIfNotExists();

            await queue.AddMessageAsync(queueMessage);
        }

        public static async Task<CloudQueueMessage> GetNextMessageFromQueue(string queueName)
        {
            var queue = GetQueueClient().GetQueueReference(queueName);

            if (await queue.ExistsAsync())
            {
                return await queue.GetMessageAsync();
            }

            return null;
        }

        public static async Task DeleteQueue(string queueName)
        {
            var queue = GetQueueClient().GetQueueReference(queueName);

            if ((await queue.ExistsAsync()))
            {
                await queue.DeleteAsync();
            };
        }

        public static CloudQueueMessage CreateQueueMessage(object messageData)
        {
            var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(messageData));

            return queueMessage;
        }

        public static T GetObjectQueueMessage<T>(CloudQueueMessage message)
        {
            var type = typeof(T);

            var queueObject = (T)JsonConvert.DeserializeObject(message.AsString, type);

            return queueObject;
        }

        private static CloudQueueClient GetQueueClient()
        {
            var storageConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;

            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            var queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry();

            return queueClient;
        }
    }
}
