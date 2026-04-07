using System.Collections.Generic;
using NUnit.Framework;
using vFrame.Core.EventDispatchers;

namespace vFrame.Core.Tests.EditMode.EventDispatchers
{
    public class EventDispatcherZeroGcTests
    {
        [Test]
        public void PublishAndDecide_NoAllocationsInSteadyState() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            dispatcher.Subscribe<TestMessage>(msg => count += msg.Value);
            dispatcher.Listen<TestDecision>(dec => {
                count += dec.Value;
                return true;
            });

            var message = new TestMessage { Value = 1 };
            var decision = new TestDecision { Value = 10 };

            Warmup(dispatcher, message, decision);

            var gcBefore = GetGcCount();
            for (var i = 0; i < 100; i++) {
                dispatcher.Publish(message);
                dispatcher.Decide(decision);
            }
            var gcAfter = GetGcCount();

            Assert.That(gcAfter, Is.EqualTo(gcBefore),
                "GC collections occurred during steady-state dispatch cycle");

            dispatcher.Destroy();
        }

        [Test]
        public void SubscribeUnsubscribePublish_NoAllocationsInSteadyState() {
            var dispatcher = CreateDispatcher();

            WarmupPool(dispatcher);

            var gcBefore = GetGcCount();
            for (var i = 0; i < 100; i++) {
                var sub = dispatcher.Subscribe<TestMessage>(_ => { });
                dispatcher.Unsubscribe(sub);
                dispatcher.Publish(new TestMessage());
            }
            var gcAfter = GetGcCount();

            Assert.That(gcAfter, Is.EqualTo(gcBefore),
                "GC collections occurred during steady-state subscribe/unsubscribe cycle");

            dispatcher.Destroy();
        }

        private static void Warmup(EventDispatcher dispatcher, TestMessage message, TestDecision decision) {
            for (var i = 0; i < 10; i++) {
                dispatcher.Publish(message);
                dispatcher.Decide(decision);
            }
        }

        private static void WarmupPool(EventDispatcher dispatcher) {
            var subs = new List<ISubscription>();
            for (var i = 0; i < 16; i++) {
                subs.Add(dispatcher.Subscribe<TestMessage>(_ => { }));
                subs.Add(dispatcher.Listen<TestDecision>(_ => true));
            }
            foreach (var sub in subs) {
                dispatcher.Unsubscribe(sub);
            }
            dispatcher.Publish(new TestMessage());
            dispatcher.Decide(new TestDecision());
        }

        private static int GetGcCount() {
            return System.GC.CollectionCount(0);
        }

        private static EventDispatcher CreateDispatcher() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class TestMessage : IInteractionMessage
        {
            public int Value { get; set; }
        }

        private sealed class TestDecision : IDecisionMessage
        {
            public int Value { get; set; }
        }
    }
}
