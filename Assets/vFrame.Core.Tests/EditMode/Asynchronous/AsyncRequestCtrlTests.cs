using NUnit.Framework;
using vFrame.Core.Unity;

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

        [Test]
        public void Update_AcrossMultipleFrames_FiresCallbacksEachFrameWithoutBleed() {
            // The finished/errored buffers are reused across frames; verify the second
            // frame does not re-fire the first frame's callbacks, and an idle frame
            // fires none (C15 buffer reuse, no cross-frame bleed).
            var ctrl = new AsyncRequestCtrl();
            ctrl.Create();
            try {
                var finishCount = 0;
                ctrl.OnRequestFinish += _ => finishCount++;

                // Frame 1: one request finishes.
                var r1 = new TestAsyncRequest();
                r1.Create();
                r1.MarkFinished();
                ctrl.AddRequest(r1);
                ctrl.Update();
                Assert.That(finishCount, Is.EqualTo(1), "frame 1 should fire one finish");

                // Frame 2: a different request finishes; the frame-1 request is gone.
                var r2 = new TestAsyncRequest();
                r2.Create();
                r2.MarkFinished();
                ctrl.AddRequest(r2);
                ctrl.Update();
                Assert.That(finishCount, Is.EqualTo(2),
                    "frame 2 should fire exactly one more finish (no re-fire of frame 1)");

                // Frame 3: nothing pending — cleared buffers must not fire stray callbacks.
                ctrl.Update();
                Assert.That(finishCount, Is.EqualTo(2),
                    "an idle frame must not fire callbacks (C15 buffer cleared)");
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
