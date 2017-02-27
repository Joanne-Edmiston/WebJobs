using Hawkmoth.Webjobs;
using Microsoft.Azure.WebJobs;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Hawkmoth.WebJobs.Test
{
    public class QueueTriggerIndexerTests
    {
        [Fact(DisplayName = "GetAllAzureQueueTriggerMethods_Finds_Only_QueueTrigger_Methods")]
        public void GetAllAzureQueueTriggerMethods_Finds_Only_QueueTrigger_Methods()
        {
            var mockTracer = new Mock<ITraceWriter>(MockBehavior.Strict);
             
            var indexer = new QueueTriggerIndexer(mockTracer.Object);

            mockTracer
                .Setup(t => t.Info(It.Is<string>(s => s == $"Found queue trigger method QueueTriggerFunction1 for queue queue1 in assembly Hawkmoth.WebJobs.Test")))
                .Verifiable();

            mockTracer
                .Setup(t => t.Info(It.Is<string>(s => s == $"Found queue trigger method QueueTriggerFunction2 for queue queue2 in assembly Hawkmoth.WebJobs.Test")))
                .Verifiable();

            mockTracer
                .Setup(t => t.Verbose(It.IsAny<string>()))
                .Verifiable();

            var methods = indexer.GetAllAzureQueueTriggerMethods();

            Assert.Equal(2, methods.Count);

            var method = methods["queue1"];
            Assert.Equal("QueueTriggerFunction1", method.Name);
            Assert.Equal(1, method.GetParameters().Length);
            Assert.Equal("queueParam1", method.GetParameters()[0].Name);

            method = methods["queue2"];
            Assert.Equal("QueueTriggerFunction2", method.Name);
            Assert.Equal(1, method.GetParameters().Length);
            Assert.Equal("queueParam2", method.GetParameters()[0].Name);

            mockTracer.VerifyAll();

        }

        [Fact(DisplayName = "GetAllAzureQueueTriggerMethods_Throws_Exception_When_Two_Methods_Triggered_By_Same_Queue", Skip = "TODO: Write Test")]
        public void GetAllAzureQueueTriggerMethods_Throws_Exception_When_Two_Methods_Triggered_By_Same_Queue()
        {
            // ToDo - write this test - try loading an asembly with invalid method into domain
        }

        [Fact(DisplayName = "GetAllAzureQueueTriggerMethods_Throws_Exception_When_QueueTrigger_Method_Has_Empty_Queue_Name", Skip = "TODO: Write Test")]
        public void GetAllAzureQueueTriggerMethods_Throws_Exception_When_QueueTrigger_Method_Has_Empty_Queue_Name()
        {
            // ToDo - write this test - try loading an asembly with invalid method into domain
        }


        /// <summary>
        /// Should be found by the indexer
        /// </summary>
        /// <param name="ob"></param>
        public static void QueueTriggerFunction1(
            [QueueTrigger("queue1")] string queueParam1)
        {
        }

        /// <summary>
        /// Should be found by the indexer
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public async static Task QueueTriggerFunction2(
            [QueueTrigger("queue2")] object queueParam2)
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
