// ------------------------------------------------------------
//         File: UniTaskBridgeTests.cs
//        Brief: Characterizes the Core IAsync -> UniTask bridge:
//               immediate completion + cancellation paths. Both avoid
//               the PlayerLoop (no UniTask.Yield is awaited), so they
//               run under EditMode.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-08-09 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using vFrame.Core;
using vFrame.Core.Unity;

namespace vFrame.Core.Tests.EditMode.Asynchronous
{
    public class UniTaskBridgeTests
    {
        private class DoneImmediatelyAsync : IAsync
        {
            public bool IsDone => true;
            public float Progress => 1f;
            public object Current => null;
            public bool MoveNext() => false;
            public void Reset() { }
        }

        private class NeverDoneAsync : IAsync
        {
            public bool IsDone => false;
            public float Progress => 0f;
            public object Current => null;
            public bool MoveNext() => true;
            public void Reset() { }
        }

        [Test]
        public void ToUniTask_Completes_WhenIAsyncIsDone()
        {
            IAsync op = new DoneImmediatelyAsync();
            // IsDone is already true -> the pump loop never runs, so the
            // await completes synchronously with no PlayerLoop dependency.
            Assert.DoesNotThrowAsync(async () => await op.ToUniTask());
        }

        [Test]
        public void ToUniTask_WithCancelledToken_ThrowsOperationCanceledException()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            IAsync neverDone = new NeverDoneAsync();
            // The token is checked before the first UniTask.Yield, so a
            // pre-cancelled token throws synchronously (no PlayerLoop needed).
            Assert.ThrowsAsync<System.OperationCanceledException>(
                async () => await neverDone.ToUniTask(cts.Token));
        }
    }
}
