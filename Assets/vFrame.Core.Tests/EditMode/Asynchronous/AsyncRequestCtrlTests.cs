using NUnit.Framework;
using vFrame.Core.Unity.Asynchronous;

namespace vFrame.Core.Tests.EditMode.Asynchronous
{
    public class AsyncRequestCtrlTests
    {
        [Test]
        public void Update_WhenRequestFinished_InvokesOnlyOnRequestFinish() {
            var ctrl = new AsyncRequestCtrl();
            ctrl.Create();

            try {
                var request = new TestAsyncRequest();
                request.Create();
                request.MarkFinished();

                var finishCount = 0;
                var errorCount = 0;
                IAsyncRequest finishedRequest = null;

                ctrl.OnRequestFinish += current => {
                    finishCount++;
                    finishedRequest = current;
                };
                ctrl.OnRequestError += _ => errorCount++;

                ctrl.AddRequest(request);
                ctrl.Update();

                Assert.That(finishCount, Is.EqualTo(1));
                Assert.That(errorCount, Is.EqualTo(0));
                Assert.That(finishedRequest, Is.SameAs(request));
            }
            finally {
                ctrl.Destroy();
            }
        }

        [Test]
        public void Update_WhenRequestErrored_InvokesOnlyOnRequestError() {
            var ctrl = new AsyncRequestCtrl();
            ctrl.Create();

            try {
                var request = new TestAsyncRequest();
                request.Create();
                request.MarkErrored();

                var finishCount = 0;
                var errorCount = 0;
                IAsyncRequest erroredRequest = null;

                ctrl.OnRequestFinish += _ => finishCount++;
                ctrl.OnRequestError += current => {
                    errorCount++;
                    erroredRequest = current;
                };

                ctrl.AddRequest(request);
                ctrl.Update();

                Assert.That(finishCount, Is.EqualTo(0));
                Assert.That(errorCount, Is.EqualTo(1));
                Assert.That(erroredRequest, Is.SameAs(request));
            }
            finally {
                ctrl.Destroy();
            }
        }

        private class TestAsyncRequest : AsyncRequest
        {
            public override float Progress => 0f;

            public void MarkFinished() {
                Finish();
            }

            public void MarkErrored() {
                Abort();
            }

            protected override void OnStart() {
            }

            protected override void OnStop() {
            }

            protected override void OnUpdate() {
            }
        }
    }
}
