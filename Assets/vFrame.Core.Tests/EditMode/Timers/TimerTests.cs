// ------------------------------------------------------------
//         File: TimerTests.cs
//        Brief: EditMode tests for <see cref="Timer"/> covering delay,
//               repeat, frame-based, pause/resume, cancel, and edge cases.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-06-07 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using NUnit.Framework;

namespace vFrame.Core.Tests
{
    [TestFixture]
    public class TimerTests
    {
        private Timer _timer;

        [SetUp]
        public void SetUp() {
            _timer = new Timer();
            _timer.Create();
        }

        [TearDown]
        public void TearDown() {
            _timer.Destroy();
        }

        [Test]
        public void Delay_FiresAfterDuration() {
            var fired = false;
            _timer.Delay(1f, () => fired = true);
            _timer.Update(0.5f);
            Assert.IsFalse(fired);
            _timer.Update(0.6f);
            Assert.IsTrue(fired);
        }

        [Test]
        public void Repeat_FiresMultipleTimes_ThenStops() {
            var count = 0;
            _timer.Repeat(1f, () => count++, 3);
            _timer.Update(1f);
            Assert.AreEqual(1, count);
            _timer.Update(1f);
            Assert.AreEqual(2, count);
            _timer.Update(1f);
            Assert.AreEqual(3, count);
            _timer.Update(1f);
            Assert.AreEqual(3, count); // stopped
        }

        [Test]
        public void Repeat_Infinite_KeepsFiring() {
            var count = 0;
            _timer.Repeat(1f, () => count++); // count: -1 (infinite)
            _timer.Update(1f);
            Assert.AreEqual(1, count);
            _timer.Update(1f);
            Assert.AreEqual(2, count);
            _timer.Update(1f);
            Assert.AreEqual(3, count);
        }

        [Test]
        public void Cancel_StopsTimer() {
            var fired = false;
            var id = _timer.Delay(1f, () => fired = true);
            _timer.Cancel(id);
            _timer.Update(2f);
            Assert.IsFalse(fired);
        }

        [Test]
        public void CancelAll_StopsAllTimers() {
            var count = 0;
            _timer.Delay(1f, () => count++);
            _timer.Delay(2f, () => count++);
            _timer.CancelAll();
            _timer.Update(3f);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void Pause_ThenResume_Continues() {
            var fired = false;
            var id = _timer.Delay(1f, () => fired = true);
            _timer.Update(0.5f);
            _timer.Pause(id);
            _timer.Update(1f);
            Assert.IsFalse(fired);
            _timer.Resume(id);
            _timer.Update(0.6f);
            Assert.IsTrue(fired);
        }

        [Test]
        public void IsRunning_ReturnsCorrectState() {
            var id = _timer.Delay(1f, () => { });
            Assert.IsTrue(_timer.IsRunning(id));
            _timer.Cancel(id);
            Assert.IsFalse(_timer.IsRunning(id));
        }

        [Test]
        public void IsRunning_NonExistentId_ReturnsFalse() {
            Assert.IsFalse(_timer.IsRunning(99999));
        }

        [Test]
        public void GetRemainingTime_ReturnsCorrectValue() {
            var id = _timer.Delay(2f, () => { });
            _timer.Update(0.5f);
            var remaining = _timer.GetRemainingTime(id);
            Assert.AreEqual(1.5f, remaining, 0.001f);
        }

        [Test]
        public void DelayFrame_FiresAfterFrames() {
            var fired = false;
            _timer.DelayFrame(3, () => fired = true);
            _timer.Update(0f); // frame 1
            Assert.IsFalse(fired);
            _timer.Update(0f); // frame 2
            Assert.IsFalse(fired);
            _timer.Update(0f); // frame 3
            Assert.IsTrue(fired);
        }
    }
}
