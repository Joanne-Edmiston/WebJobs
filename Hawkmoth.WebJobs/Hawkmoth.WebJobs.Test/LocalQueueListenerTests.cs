﻿using Hawkmoth.Webjobs;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Queue;
using Moq;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hawkmoth.WebJobs.Test
{
    /// <summary>
    /// The tests in this class require the local storage emulator to be started
    /// </summary>
    public class LocalQueueListenerTests
    {
        private const string testQueueName = "testqueuename";

        [Fact(DisplayName = "StartListening_Does_Not_Throw_Exception_If_Queue_Not_Yet_Created")]
        public void StartListening_Does_Not_Throw_Exception_If_Queue_Not_Yet_Created()
        {
            var mockTracer = new Mock<ITraceWriter>(MockBehavior.Strict);
            var mockInvoker = new Mock<IQueueTriggerMethodInvoker>(MockBehavior.Strict);
            var pollIntervalSeconds = 5;

            var listener = new LocalQueueListener(mockTracer.Object, mockInvoker.Object, pollIntervalSeconds);
            var missingQueueName = "missingqueue";

            var method = typeof(QueueListenerTestFunctions).GetMethods().FirstOrDefault();

            mockTracer
                .Setup(t => t.Info(It.Is<string>(s => s == $"Start Listening on queue '{missingQueueName}'")))
                .Verifiable();

            mockTracer
                .Setup(t => t.Verbose(It.Is<string>(s => s == $"Queue '{missingQueueName}' does not exist.")))
                .Verifiable();

            mockTracer
                .Setup(t => t.Info(It.Is<string>(s => s == $"Stop listening to queues")))
                .Verifiable();


            listener.StartListening(missingQueueName, method);

            // sleep for 1 second before poll interval
            Thread.Sleep((pollIntervalSeconds -1) * 1000);

            listener.Stop();

            mockTracer.VerifyAll();
            mockInvoker
                .Verify(t => t.InvokeAsync(It.IsAny<CloudQueueMessage>(), It.IsAny<MethodInfo>()), Times.Never());
        }


        [Fact(DisplayName = "StartListening_Throws_Exception_If_QueueName_Is_Empty")]
        public void StartListening_Throws_Exception_If_QueueName_Is_Empty()
        {
            var mockTracer = new Mock<ITraceWriter>(MockBehavior.Strict);
            var mockInvoker = new Mock<IQueueTriggerMethodInvoker>(MockBehavior.Strict);
            var pollIntervalSeconds = 5;

            var listener = new LocalQueueListener(mockTracer.Object, mockInvoker.Object, pollIntervalSeconds);

            var method = typeof(QueueListenerTestFunctions).GetMethods().FirstOrDefault();

            var ex = Assert.Throws<ArgumentException>(() => listener.StartListening("", method));

            Assert.Equal("queueName must not be null or empty", ex.Message);

            mockInvoker
               .Verify(t => t.InvokeAsync(It.IsAny<CloudQueueMessage>(), It.IsAny<MethodInfo>()), Times.Never());

        }

        [Fact(DisplayName = "StartListening_Throws_Exception_If_MethodInfo_is_null")]
        public void StartListening_Throws_Exception_If_MethodInfo_is_null()
        {
            var mockTracer = new Mock<ITraceWriter>(MockBehavior.Strict);
            var mockInvoker = new Mock<IQueueTriggerMethodInvoker>(MockBehavior.Strict);
            var pollIntervalSeconds = 5;

            var listener = new LocalQueueListener(mockTracer.Object, mockInvoker.Object, pollIntervalSeconds);

            var method = typeof(QueueListenerTestFunctions).GetMethods().FirstOrDefault();

            var ex = Assert.Throws<ArgumentNullException>(() => listener.StartListening("test", null));

            Assert.Equal("queueTriggerMethod", ex.ParamName);

            mockInvoker
               .Verify(t => t.InvokeAsync(It.IsAny<CloudQueueMessage>(), It.IsAny<MethodInfo>()), Times.Never());

        }


        [Fact(DisplayName = "StartListening_Invokes_Queue_Trigger_Method_If_Item_Found_In_Queue")]
        public async Task StartListening_Invokes_Queue_Trigger_Method_If_Item_Found_In_Queue()
        {
            var mockTracer = new Mock<ITraceWriter>(MockBehavior.Strict);
            var mockInvoker = new Mock<IQueueTriggerMethodInvoker>(MockBehavior.Strict);
            var pollIntervalSeconds = 5;

            var listener = new LocalQueueListener(mockTracer.Object, mockInvoker.Object, pollIntervalSeconds);

            var method = typeof(QueueListenerTestFunctions).GetMethods().FirstOrDefault();

            // Add test message to the queue
            var queueData = new TestQueueParameterType
            {
                TestProperty = "This is a test object to be queued"
            };
            await QueueUtilities.DeleteQueue(testQueueName);
            await QueueUtilities.QueueMessageDataAsync(testQueueName, queueData);

            mockTracer
                .Setup(t => t.Info(It.Is<string>(s => s == $"Start Listening on queue '{testQueueName}'")))
                .Verifiable();

            mockTracer
                .Setup(t => t.Info(It.Is<string>(s => s == $"Processing new message from '{testQueueName}'")))
                .Verifiable();

            mockInvoker
                .Setup(i => i.InvokeAsync(
                    It.Is<CloudQueueMessage>(m => VerifyCloudQueueMessasge(m, queueData)),
                    It.Is<MethodInfo>(m => m == method)))
                .Verifiable();

            mockTracer
                .Setup(t => t.Info(It.Is<string>(s => s == $"Stop listening to queues")))
                .Verifiable();


            listener.StartListening(testQueueName, method);

            // sleep for 1 second before poll interval
            Thread.Sleep((pollIntervalSeconds - 1) * 1000);

            listener.Stop();

            mockTracer.VerifyAll();
            mockInvoker.VerifyAll();
        }

        [Fact(DisplayName = "StartListening_Deletes_Queue_Message_After_Processing", Skip = "TODO: write test")]
        public async Task StartListening_Deletes_Queue_Message_After_Processing()
        {

        }

        [Fact(DisplayName = "StartListening_Inovkes_Queue_Trigger_Method_For_All_Items_In_Queue_Batch")]
        public async Task StartListening_Inovkes_Queue_Trigger_Method_For_All_Items_In_Queue_Batch()
        {
            var mockTracer = new Mock<ITraceWriter>(MockBehavior.Strict);
            var mockInvoker = new Mock<IQueueTriggerMethodInvoker>(MockBehavior.Strict);

            var pollIntervalSeconds = 5;

            var listener = new LocalQueueListener(mockTracer.Object, mockInvoker.Object, pollIntervalSeconds);

            var method = typeof(QueueListenerTestFunctions).GetMethods().FirstOrDefault();

            // Add test message to the queue
            var queueData1 = new TestQueueParameterType { TestProperty = "Test Data 1" };
            var queueData2 = new TestQueueParameterType { TestProperty = "Test Data 2" };
            var queueData3 = new TestQueueParameterType { TestProperty = "Test Data 3" };

            await QueueUtilities.DeleteQueue(testQueueName);

            await QueueUtilities.QueueMessageDataAsync(testQueueName, queueData1);
            await QueueUtilities.QueueMessageDataAsync(testQueueName, queueData2);
            await QueueUtilities.QueueMessageDataAsync(testQueueName, queueData3);

            mockTracer.Setup(t => t.Info(It.Is<string>(s => s == $"Start Listening on queue '{testQueueName}'"))).Verifiable();

            mockTracer.Setup(t => t.Info(It.Is<string>(s => s == $"Processing new message from '{testQueueName}'"))).Verifiable();

            mockInvoker.Setup(i => i.InvokeAsync(It.Is<CloudQueueMessage>(m => VerifyCloudQueueMessasge(m, queueData1)), It.Is<MethodInfo>(m => m == method))).Verifiable();
            mockInvoker.Setup(i => i.InvokeAsync(It.Is<CloudQueueMessage>(m => VerifyCloudQueueMessasge(m, queueData2)), It.Is<MethodInfo>(m => m == method))).Verifiable();
            mockInvoker.Setup(i => i.InvokeAsync(It.Is<CloudQueueMessage>(m => VerifyCloudQueueMessasge(m, queueData3)), It.Is<MethodInfo>(m => m == method))).Verifiable();

            mockTracer.Setup(t => t.Info(It.Is<string>(s => s == $"Stop listening to queues"))).Verifiable();

            listener.StartListening(testQueueName, method);

            // sleep for 1 second before poll interval
            Thread.Sleep((pollIntervalSeconds - 1) * 1000);

            listener.Stop();

            mockTracer.VerifyAll();
            mockTracer.Verify(t => t.Info(It.Is<string>(s => s == $"Processing new message from '{testQueueName}'")), Times.Exactly(3));
            mockInvoker.VerifyAll();
        }


        [Fact(DisplayName = "StartListening_Does_Not_Poll_Queue_If_Listener_Is_Stopping", Skip ="TODO: Write test")]
        public void StartListening_Does_Not_Poll_Queue_If_Listener_Is_Stopping()
        {
        }

        [Fact(DisplayName = "Stop_Stops_All_Queue_Polling", Skip = "TODO: Write test")]
        public void Stop_Stops_All_Queue_Polling()
        {
        }




        private bool VerifyCloudQueueMessasge(CloudQueueMessage message, TestQueueParameterType expectedMessageObject)
        {
            var actualMessageObject = QueueUtilities.GetObjectQueueMessage<TestQueueParameterType>(message);

            Assert.Equal(expectedMessageObject.TestProperty, actualMessageObject.TestProperty);

            return true;
        }

        /// <summary>
        /// Private class with queue method to invoke in unit tests
        /// (as it is private it will not be found by the indexer)
        /// </summary>
        private class QueueListenerTestFunctions
        {
            public static void TestMethod(
                [QueueTrigger(testQueueName)] TestQueueParameterType queueTriggerParam)
            {
            }

        }

        private class TestQueueParameterType
        {
            public string TestProperty { get; set; }
        }
    }
}
