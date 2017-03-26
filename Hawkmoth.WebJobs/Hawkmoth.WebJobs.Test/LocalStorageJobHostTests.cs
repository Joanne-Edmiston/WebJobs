using Hawkmoth.Webjobs;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hawkmoth.WebJobs.Test
{
    public class LocalStorageJobHostTests
    {
        [Fact(DisplayName = "LocalStorageJobHost_Start_Starts_Listener_For_Each_Queue")]
        public static void LocalStorageJobHost_Start_Starts_Listener_For_Each_Queue()
        {
            var mockInexer = new Mock<IQueueTriggerIndexer>(MockBehavior.Strict);
            var mocklistener = new Mock<IQueueListener>(MockBehavior.Strict);

            var queueName1 = "testqueue1";
            var queueName2 = "testqueue2";
            var method1 = typeof(TestFunctions).GetMethods().First();
            var method2 = typeof(TestFunctions).GetMethods().First();

            var indexedMethods = new Dictionary<string, MethodInfo>();
            indexedMethods.Add(queueName1, method1);
            indexedMethods.Add(queueName2, method2);

            mockInexer.Setup(i => i.GetAllAzureQueueTriggerMethods())
                .Returns(indexedMethods);

            mocklistener.Setup(l => l.StartListening(
                It.Is<string>(s => s == queueName1),
                It.Is<MethodInfo>(m => m == method1)));

            mocklistener.Setup(l => l.StartListening(
                It.Is<string>(s => s == queueName2),
                It.Is<MethodInfo>(m => m == method2)));

            var localStorageHost = new LocalStorageJobHost(
                queueTriggerIndexer: mockInexer.Object, queueListener: mocklistener.Object);

            localStorageHost.Start();

            mockInexer.VerifyAll();
            mocklistener.VerifyAll();

        }


        [Fact(DisplayName = "LocalStorageJobHost_Stop_Calls_StopAll_On_Queue_Listener_If_Not_Already_Stopping")]
        public static void LocalStorageJobHost_Stop_Calls_StopAll_On_Queue_Listener_If_Not_Already_Stopping()
        {
            var mockInexer = new Mock<IQueueTriggerIndexer>(MockBehavior.Strict);
            var mocklistener = new Mock<IQueueListener>(MockBehavior.Strict);

            mocklistener.Setup(l => l.IsStopping).Returns(false);
            mocklistener.Setup(l => l.StopAll());

            var localStorageJobHost = new LocalStorageJobHost(
                queueTriggerIndexer: mockInexer.Object, queueListener: mocklistener.Object);


            localStorageJobHost.Stop();

            mocklistener.VerifyAll();

        }

        [Fact(DisplayName = "LocalStorageJobHost_Stop_Does_Not_Call_StopAll_On_Queue_Listener_If_Stopping")]
        public static void LocalStorageJobHost_Stop_Does_Not_Call_StopAll_On_Queue_Listener_If_Stopping()
        {
            var mockInexer = new Mock<IQueueTriggerIndexer>(MockBehavior.Strict);
            var mocklistener = new Mock<IQueueListener>(MockBehavior.Strict);

            mocklistener.Setup(l => l.IsStopping).Returns(true);

            var localStorageJobHost = new LocalStorageJobHost(
                queueTriggerIndexer: mockInexer.Object, queueListener: mocklistener.Object);


            localStorageJobHost.Stop();

            mocklistener.Verify(l => l.StopAll(), Times.Never());

            mocklistener.VerifyAll();

        }
    }
}
