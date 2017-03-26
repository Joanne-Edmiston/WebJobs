using Hawkmoth.Webjobs;
using Microsoft.Azure.WebJobs;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hawkmoth.WebJobs.Test
{
    public class QueueTriggerMethodInvokerTests
    {
        [Fact(DisplayName = "QueueTriggerMethodInvoker_InvokeAsync_Invokes_Non_Async_Method_Correctly")]
        public async Task QueueTriggerMethodInvoker_InvokeAsync_Invokes_Async_Method_Correctly()
        {
            var nonAsyncMethod = typeof(TestFunctions).GetMethods().First(m => m.Name == nameof(TestFunctions.QueueTriggerFunction2));
            var param = "Test Parameter";
            var queueMessage = QueueUtilities.CreateQueueMessage(param);

            var mockTracer = new Mock<ITraceWriter>(MockBehavior.Strict);

            TestFunctions.TraceWriter = mockTracer.Object;

            mockTracer.Setup(t => t.Info(It.Is<string>(m => m == $"QueueTriggerFunction2 called with parameter {param}")));

            var invoker = new QueueTriggerMethodInvoker();

            await invoker.InvokeAsync(queueMessage, nonAsyncMethod);

            mockTracer.VerifyAll();
        }

        [Fact(DisplayName = "QueueTriggerMethodInvoker_InvokeAsync_Invokes_Non_Async_Method_Correctly")]
        public async Task QueueTriggerMethodInvoker_InvokeAsync_Invokes_Non_Async_Method_Correctly()
        {
            var nonAsyncMethod = typeof(TestFunctions).GetMethods().First(m => m.Name == nameof(TestFunctions.QueueTriggerFunction1));
            var param = "Test Parameter";
            var queueMessage = QueueUtilities.CreateQueueMessage(param);

            var mockTracer = new Mock<ITraceWriter>(MockBehavior.Strict);

            TestFunctions.TraceWriter = mockTracer.Object;

            mockTracer.Setup(t => t.Info(It.Is<string>(m => m == $"QueueTriggerFunction1 called with parameter {param}")));

            var invoker = new QueueTriggerMethodInvoker();

            await invoker.InvokeAsync(queueMessage, nonAsyncMethod);

            mockTracer.VerifyAll();

        }


    }
}
