using NUnit.Framework;
using vFrame.Core.Base;
using vFrame.Core.Dispatchers;
using vFrame.Core.Exceptions;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    public class DispatcherInteractionTests
    {
        [Test]
        public void Subscribe_PublishUnsubscribe_Publish_OnlyFirstInvoked() {
            var dispatcher = CreateDispatcher();
            var first = new CountingMessageHandler();
            var second = new CountingMessageHandler();

            var firstSub = dispatcher.Subscribe<TestEvent>(first.OnMessage);
            dispatcher.Subscribe<TestEvent>(second.OnMessage);

            dispatcher.Publish(new TestEvent { Value = 1 });
            dispatcher.Unsubscribe(firstSub);
            dispatcher.Publish(new TestEvent { Value = 2 });

            Assert.That(first.Count, Is.EqualTo(1));
            Assert.That(second.Count, Is.EqualTo(2));
        }

        [Test]
        public void Subscribe_BareSubscriptionRemainsActive_UntilExplicitRemoval() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner();
            owner.Create();
            var bare = new CountingMessageHandler();

            var bareSub = dispatcher.Subscribe<TestEvent>(bare.OnMessage);
            owner.Bind(dispatcher);

            owner.Destroy();
            dispatcher.Publish(new TestEvent { Value = 7 });
            dispatcher.Unsubscribe(bareSub);
            dispatcher.Publish(new TestEvent { Value = 8 });

            Assert.That(owner.Listener.Count, Is.EqualTo(0));
            Assert.That(bare.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void GetDiagnostics_ReflectsCurrentCounts() {
            dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Listen<TestDecision>(_ => true);

            var diagnostics = dispatcher.GetDiagnostics();

            Assert.That(diagnostics.EventSubscriptionCount, Is.EqualTo(1));
            Assert.That(diagnostics.DecisionSubscriptionCount, Is.EqualTo(1));

            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_RemovesOwnerBoundSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner();
            owner.Create();

            owner.Bind(dispatcher);
            dispatcher.Publish(new TestEvent());

            owner.Destroy();
            dispatcher.Publish(new TestEvent());

            Assert.That(owner.Listener.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_RemovesLifetimeBoundSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeOwnedSubscriptionOwner();
            owner.Create();

            owner.Bind(dispatcher);
            dispatcher.Publish(new TestEvent { Value = 1 });

            owner.EndGroup();
            dispatcher.Publish(new TestEvent { Value = 2 });

            Assert.That(owner.Listener.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));

            owner.Destroy();
            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_ClearsOwnedState_AndPreventsFurtherUse() {
            var dispatcher = CreateDispatcher();
            var eventCount = 0;

            dispatcher.Subscribe<TestEvent>(_ => eventCount++);
            dispatcher.Listen<TestDecision>(_ => true);

            dispatcher.Destroy();

            Assert.That(dispatcher.Destroyed, Is.True);
            Assert.That(() => dispatcher.Publish(new TestEvent()), Throws.TypeOf<BaseObjectDestroyedException>());
            Assert.That(() => dispatcher.Decide(new TestDecision()), Throws.TypeOf<BaseObjectDestroyedException>());
            Assert.That(eventCount, Is.EqualTo(0));
        }

        [Test]
        public void RemoveAllSubscriptions_ClearsAllPaths() {
            var dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Listen<TestDecision>(_ => true);

            dispatcher.RemoveAllSubscriptions();

            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));
            Assert.That(dispatcher.GetDecisionSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void GetTotalSubscriptionCount_SumsAllPaths() {
            var dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Listen<TestDecision>(_ => true);
            dispatcher.Listen<TestDecision>(_ => true);
            dispatcher.Listen<TestDecision>(_ => true);

            Assert.That(dispatcher.GetTotalSubscriptionCount(), Is.EqualTo(5));

            dispatcher.Destroy();
        }

        private Dispatcher dispatcher;

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
        }

        private sealed class CountingMessageHandler
        {
            public int Count { get; private set; }

            public object LastValue { get; private set; }

            public void OnMessage(TestEvent message) {
                Count++;
                LastValue = message.Value;
            }
        }

        private sealed class SubscriptionOwner : BaseObject
        {
            public CountingMessageHandler Listener { get; } = new CountingMessageHandler();

            public void Bind(Dispatcher dispatcher) {
                var subscription = dispatcher.Subscribe<TestEvent>(Listener.OnMessage, this);
            }

            protected override void OnCreate() {
            }

            protected override void OnDestroy() {
            }
        }

        private sealed class LifetimeOwnedSubscriptionOwner : BaseObject
        {
            private ILifetime _group;

            public CountingMessageHandler Listener { get; } = new CountingMessageHandler();

            public void EndGroup() {
                _group.Destroy();
            }

            public void Bind(Dispatcher dispatcher) {
                dispatcher.Subscribe<TestEvent>(Listener.OnMessage, _group);
            }

            protected override void OnCreate() {
                _group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() {
            }
        }
    }
}
