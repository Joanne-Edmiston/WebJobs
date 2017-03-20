using Hawkmoth.Webjobs;
using Moq;
using Xunit;

namespace Hawkmoth.WebJobs.Test
{
    public class QueueTriggerIndexerTests
    {
        [Fact(DisplayName = "QueueTriggerIndexer_GetAllAzureQueueTriggerMethods_Finds_Only_QueueTrigger_Methods")]
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

        [Fact(DisplayName = "QueueTriggerIndexer_GetAllAzureQueueTriggerMethods_Throws_Exception_When_Two_Methods_Triggered_By_Same_Queue", Skip = "TODO: Write Test")]
        public void GetAllAzureQueueTriggerMethods_Throws_Exception_When_Two_Methods_Triggered_By_Same_Queue()
        {
            // ToDo - write this test - try loading an asembly with invalid method into domain
        }

        [Fact(DisplayName = "QueueTriggerIndexer_GetAllAzureQueueTriggerMethods_Throws_Exception_When_QueueTrigger_Method_Has_Empty_Queue_Name", Skip = "TODO: Write Test")]
        public void GetAllAzureQueueTriggerMethods_Throws_Exception_When_QueueTrigger_Method_Has_Empty_Queue_Name()
        {
            // ToDo - write this test - try loading an asembly with invalid method into domain
        }


      
    }
}
