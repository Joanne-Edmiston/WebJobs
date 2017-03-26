using Hawkmoth.Webjobs;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;

namespace Hawkmoth.WebJobs.Test
{
    /// <summary>
    ///  Test functions for unit tests
    /// </summary>
    public class TestFunctions
    {
        public static ITraceWriter TraceWriter;

        /// <summary>
        /// Should be found by the indexer
        /// </summary>
        /// <param name="queueParam1"></param>
        public static void QueueTriggerFunction1(
            [QueueTrigger("queue1")] string queueParam1)
        {
            TraceWriter.Info($"QueueTriggerFunction1 called with parameter {queueParam1}"); 
        }

        /// <summary>
        /// Should be found by the indexer
        /// </summary>
        /// <param name="queueParam2"></param>
        /// <returns></returns>
        public async static Task QueueTriggerFunction2(
            [QueueTrigger("queue2")] object queueParam2)
        {
            await Task.Run(() =>
            {
                TraceWriter.Info($"QueueTriggerFunction2 called with parameter {queueParam2}");
            });
        }

        /// <summary>
        /// Should not be found by the indexer, as the method is private
        /// </summary>
        /// <param name="queueParam3"></param>
        /// <returns></returns>
        private async static Task PrivateFunction(
           [QueueTrigger("queue3")] object queueParam3)
        {

        }

        /// <summary>
        /// Should not be found by the indexer
        /// </summary>
        /// <param name="ob"></param>
        public static void NotAQueueTriggerFunction(object ob)
        {
        }

    }
}
