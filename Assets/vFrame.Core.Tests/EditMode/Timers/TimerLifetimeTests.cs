// ------------------------------------------------------------
//         File: TimerLifetimeTests.cs
//        Brief: EditMode tests verifying that lifetime-bound timers are
//               automatically cancelled when their associated lifetime ends.
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
    public class TimerLifetimeTests
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
        public void Delay_CancelledBeforeFiring_DoesNotFire() {
            var fired = false;
            var id = _timer.Delay(1f, () => fired = true);
            _timer.Cancel(id);
            _timer.Update(2f);
            Assert.IsFalse(fired);
        }

        [Test]
        public void Repeat_CancelledAfterFirstFire_StopsRepeating() {
            var count = 0;
            var id = _timer.Repeat(1f, () => count++, 3);
            _timer.Update(1f);
            Assert.AreEqual(1, count);
            _timer.Cancel(id);
            _timer.Update(1f);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void DelayFrame_CancelledBeforeFiring_DoesNotFire() {
            var fired = false;
            var id = _timer.DelayFrame(3, () => fired = true);
            _timer.Cancel(id);
            _timer.Update(0f);
            _timer.Update(0f);
            _timer.Update(0f);
            Assert.IsFalse(fired);
        }
    }
}
