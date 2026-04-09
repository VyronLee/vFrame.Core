using System;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    public class DispatcherDecisionTests
    {
        [Test]
        public void Decide_AllHandlersApprove_ReturnsTrue() {
            var dispatcher = CreateDispatcher();
            var first = new CountingDecisionHandler(true);
            var second = new CountingDecisionHandler(true);

            dispatcher.Listen<TestDecision>(first.OnDecision);
            dispatcher.Listen<TestDecision>(second.OnDecision);

            var passed = dispatcher.Decide(new TestDecision { Value = 42 });

            Assert.That(passed, Is.True);
            Assert.That(first.CallCount, Is.EqualTo(1));
            Assert.That(second.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Decide_FirstHandlerRejects_StopsAndReturnsFalse() {
            var dispatcher = CreateDispatcher();
            var first = new CountingDecisionHandler(true);
            var second = new CountingDecisionHandler(false);
            var third = new CountingDecisionHandler(true);

            dispatcher.Listen<TestDecision>(first.OnDecision);
            dispatcher.Listen<TestDecision>(second.OnDecision);
            dispatcher.Listen<TestDecision>(third.OnDecision);

            var passed = dispatcher.Decide(new TestDecision { Value = 1 });

            Assert.That(passed, Is.False);
            Assert.That(first.CallCount, Is.EqualTo(1));
            Assert.That(second.CallCount, Is.EqualTo(1));
            Assert.That(third.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Decide_NoListeners_ReturnsTrue() {
            var dispatcher = CreateDispatcher();

            var passed = dispatcher.Decide(new TestDecision());

            Assert.That(passed, Is.True);
        }

        [Test]
        public void Decide_ReceivesDecisionPayload() {
            var dispatcher = CreateDispatcher();
            int received = 0;

            dispatcher.Listen<TestDecision>(d => {
                received = d.Value;
                return true;
            });

            dispatcher.Decide(new TestDecision { Value = 99 });

            Assert.That(received, Is.EqualTo(99));
        }

        [Test]
        public void Listen_ReturnsSubscription() {
            var dispatcher = CreateDispatcher();

            var subscription = dispatcher.Listen<TestDecision>(_ => true);

            Assert.That(subscription, Is.Not.Null);
            Assert.That(subscription, Is.InstanceOf<ISubscription>());
            Assert.That(subscription.Handle, Is.GreaterThan(0));
        }

        [Test]
        public void Unlisten_RemovesDecisionSubscription() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            var subscription = dispatcher.Listen<TestDecision>(_ => {
                count++;
                return true;
            });

            dispatcher.Decide(new TestDecision());
            dispatcher.Unlisten(subscription);
            dispatcher.Decide(new TestDecision());

            Assert.That(count, Is.EqualTo(1));
            Assert.That(dispatcher.GetDecisionSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void OwnerDestroy_RemovesDecisionSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new DecisionOwner();
            owner.Create();

            dispatcher.Listen<TestDecision>(owner.OnDecision, owner);
            dispatcher.Decide(new TestDecision { Value = 1 });
            owner.Destroy();
            dispatcher.Decide(new TestDecision { Value = 2 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetDecisionSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void LifetimeDestroy_RemovesDecisionSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeDecisionOwner();
            owner.Create();

            dispatcher.Listen<TestDecision>(owner.OnDecision, owner.Group);
            dispatcher.Decide(new TestDecision { Value = 1 });
            owner.Group.Destroy();
            dispatcher.Decide(new TestDecision { Value = 2 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetDecisionSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void Decide_ExceptionIsolation_ContinuesEvaluating() {
            var dispatcher = CreateDispatcher();
            var secondCalled = false;

            dispatcher.Listen<TestDecision>(_ => throw new Exception("test"));
            dispatcher.Listen<TestDecision>(_ => {
                secondCalled = true;
                return true;
            });

            var passed = dispatcher.Decide(new TestDecision());

            Assert.That(secondCalled, Is.True);
            Assert.That(passed, Is.True);
        }

        [Test]
        public void Decide_RemainsDistinctFromEventDispatch() {
            var dispatcher = CreateDispatcher();
            var decisionCount = 0;
            var eventCount = 0;

            dispatcher.Listen<TestDecision>(_ => {
                decisionCount++;
                return true;
            });
            dispatcher.Subscribe<TestEvent>(_ => eventCount++);

            var passed = dispatcher.Decide(new TestDecision());
            dispatcher.Publish(new TestEvent());

            Assert.That(passed, Is.True);
            Assert.That(decisionCount, Is.EqualTo(1));
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void Destroy_ClearsDecisionSubscriptions() {
            var dispatcher = CreateDispatcher();

            dispatcher.Listen<TestDecision>(_ => true);
            dispatcher.Destroy();

            Assert.That(dispatcher.Destroyed, Is.True);
            Assert.That(() => dispatcher.Decide(new TestDecision()), Throws.TypeOf<BaseObjectDestroyedException>());
        }

        private static Dispatcher CreateDispatcher() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class TestDecision : IDecision
        {
            public int Value { get; set; }
        }

        private sealed class TestEvent : IEvent
        {
        }

        private sealed class CountingDecisionHandler
        {
            private readonly bool _result;

            public CountingDecisionHandler(bool result) {
                _result = result;
            }

            public int CallCount { get; private set; }

            public bool OnDecision(TestDecision decision) {
                CallCount++;
                return _result;
            }
        }

        private sealed class DecisionOwner : BaseObject
        {
            public int Count { get; private set; }

            public bool OnDecision(TestDecision decision) {
                Count += decision.Value;
                return true;
            }

            protected override void OnCreate() {
            }

            protected override void OnDestroy() {
            }
        }

        private sealed class LifetimeDecisionOwner : BaseObject
        {
            public int Count { get; private set; }

            public ILifetime Group { get; private set; }

            public bool OnDecision(TestDecision decision) {
                Count += decision.Value;
                return true;
            }

            protected override void OnCreate() {
                Group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() {
            }
        }
    }
}