using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using vFrame.Core.MultiThreading;

namespace vFrame.Core.Tests.EditMode
{
    public class ThreadedTaskFaultTerminationTests
    {
        [Test]
        public void FaultedWorker_EventuallyReachesDoneState() {
            var task = new ThrowingThreadedTask();

            task.Create(1);

            var timeout = Stopwatch.StartNew();
            while (!task.IsDone && timeout.Elapsed < TimeSpan.FromSeconds(1)) {
                Thread.Sleep(10);
            }

            Assert.That(task.IsDone, Is.True, "Faulted threaded task should eventually become done.");
            Assert.That(task.Progress, Is.EqualTo(1f), "Faulted threaded task should report terminal progress.");
            Assert.That(task.CapturedException, Is.TypeOf<InvalidOperationException>());
        }

        private sealed class ThrowingThreadedTask : ThreadedTask<int>
        {
            public Exception CapturedException { get; private set; }

            protected override void ErrorHandler(Exception e) {
                CapturedException = e;
            }

            protected override void OnHandleTask(int arg) {
                throw new InvalidOperationException("Expected test failure.");
            }
        }
    }
}
