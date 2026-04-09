using System.Collections.Generic;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    public class DispatcherZeroGcTests
    {
        [Test]
        public void PublishAndDecide_NoAllocationsInSteadyState() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            dispatcher.Subscribe<TestEvent>(msg => count += msg.Value);
            dispatcher.Listen<TestDecision>(dec => {
                count += dec.Value;
                return true;
            });

            var message = new TestEvent { Value = 1 };
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
        public void SendAndRequest_NoAllocationsInSteadyState() {
            var dispatcher = CreateDispatcher();

            dispatcher.Handle<TestCommand>(cmd => { });
            dispatcher.HandleRequest<TestRequest, int>(req => req.Value * 2);

            var command = new TestCommand { Value = 5 };
            var request = new TestRequest { Value = 3 };

            for (var i = 0; i < 10; i++) {
                dispatcher.Send(command);
                dispatcher.Request<TestRequest, int>(request);
            }

            var gcBefore = GetGcCount();
            for (var i = 0; i < 100; i++) {
                dispatcher.Send(command);
                dispatcher.Request<TestRequest, int>(request);
                dispatcher.TryRequest<TestRequest, int>(request, out _);
            }
            var gcAfter = GetGcCount();

            Assert.That(gcAfter, Is.EqualTo(gcBefore),
                "GC collections occurred during steady-state command/request cycle");

            dispatcher.Destroy();
        }

        [Test]
        public void SubscribeUnsubscribePublish_NoAllocationsInSteadyState() {
            var dispatcher = CreateDispatcher();

            WarmupPool(dispatcher);

            var gcBefore = GetGcCount();
            for (var i = 0; i < 100; i++) {
                var sub = dispatcher.Subscribe<TestEvent>(_ => { });
                dispatcher.Unsubscribe(sub);
                dispatcher.Publish(new TestEvent());
            }
            var gcAfter = GetGcCount();

            Assert.That(gcAfter, Is.EqualTo(gcBefore),
                "GC collections occurred during steady-state subscribe/unsubscribe cycle");

            dispatcher.Destroy();
        }

        private static void Warmup(Dispatcher dispatcher, TestEvent message, TestDecision decision) {
            for (var i = 0; i < 10; i++) {
                dispatcher.Publish(message);
                dispatcher.Decide(decision);
            }
        }

        private static void WarmupPool(Dispatcher dispatcher) {
            var subs = new List<ISubscription>();
            for (var i = 0; i < 16; i++) {
                subs.Add(dispatcher.Subscribe<TestEvent>(_ => { }));
                subs.Add(dispatcher.Listen<TestDecision>(_ => true));
            }
            foreach (var sub in subs) {
                dispatcher.Unsubscribe(sub);
            }
            dispatcher.Publish(new TestEvent());
            dispatcher.Decide(new TestDecision());
        }

        private static int GetGcCount() {
            return System.GC.CollectionCount(0);
        }

        private static Dispatcher CreateDispatcher() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class TestEvent : IEvent
        {
            public int Value { get; set; }
        }

        private sealed class TestDecision : IDecision
        {
            public int Value { get; set; }
        }

        private sealed class TestCommand : ICommand
        {
            public int Value { get; set; }
        }

        private sealed class TestRequest : IRequest<int>
        {
            public int Value { get; set; }
        }
    }
}