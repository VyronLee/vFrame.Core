using NUnit.Framework;
using vFrame.Core.Base;
using vFrame.Core.EventDispatchers;
using vFrame.Core.Exceptions;

namespace vFrame.Core.Tests.EditMode.EventDispatchers
{
    public class EventDispatcherInteractionTests
    {
        [Test]
        public void Subscribe_PublishUnsubscribe_Publish_OnlyFirstInvoked() {
            var dispatcher = CreateDispatcher();
            var first = new CountingMessageHandler();
            var second = new CountingMessageHandler();

            var firstSub = dispatcher.Subscribe<TestMessage>(first.OnMessage);
            dispatcher.Subscribe<TestMessage>(second.OnMessage);

            dispatcher.Publish(new TestMessage { Value = 1 });
            dispatcher.Unsubscribe(firstSub);
            dispatcher.Publish(new TestMessage { Value = 2 });

            Assert.That(first.Count, Is.EqualTo(1));
            Assert.That(second.Count, Is.EqualTo(2));
        }

        [Test]
        public void Subscribe_BareSubscriptionRemainsActive_UntilExplicitRemoval() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner();
            owner.Create();
            var bare = new CountingMessageHandler();

            var bareSub = dispatcher.Subscribe<TestMessage>(bare.OnMessage);
            owner.Bind(dispatcher);

            owner.Destroy();
            dispatcher.Publish(new TestMessage { Value = 7 });
            dispatcher.Unsubscribe(bareSub);
            dispatcher.Publish(new TestMessage { Value = 8 });

            Assert.That(owner.Listener.Count, Is.EqualTo(0));
            Assert.That(bare.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void GetDiagnostics_ReflectsCurrentCounts() {
            var dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestMessage>(_ => { });
            dispatcher.Listen<TestDecision>(_ => true);

            var diagnostics = dispatcher.GetDiagnostics();

            Assert.That(diagnostics.InteractionSubscriptionCount, Is.EqualTo(1));
            Assert.That(diagnostics.DecisionSubscriptionCount, Is.EqualTo(1));

            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_RemovesOwnerBoundSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner();
            owner.Create();

            owner.Bind(dispatcher);
            dispatcher.Publish(new TestMessage());

            owner.Destroy();
            dispatcher.Publish(new TestMessage());

            Assert.That(owner.Listener.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_RemovesLifetimeBoundSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeOwnedSubscriptionOwner();
            owner.Create();

            owner.Bind(dispatcher);
            dispatcher.Publish(new TestMessage { Value = 1 });

            owner.EndGroup();
            dispatcher.Publish(new TestMessage { Value = 2 });

            Assert.That(owner.Listener.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(0));

            owner.Destroy();
            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_ClearsOwnedState_AndPreventsFurtherUse() {
            var dispatcher = CreateDispatcher();
            var typedCount = 0;

            dispatcher.Subscribe<TestMessage>(_ => typedCount++);
            dispatcher.Listen<TestDecision>(_ => true);

            dispatcher.Destroy();

            Assert.That(dispatcher.Destroyed, Is.True);
            Assert.That(() => dispatcher.Publish(new TestMessage()), Throws.TypeOf<BaseObjectDestroyedException>());
            Assert.That(() => dispatcher.Decide(new TestDecision()), Throws.TypeOf<BaseObjectDestroyedException>());
            Assert.That(typedCount, Is.EqualTo(0));
        }

        [Test]
        public void RemoveAllSubscriptions_ClearsBothPaths() {
            var dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestMessage>(_ => { });
            dispatcher.Subscribe<TestMessage>(_ => { });
            dispatcher.Listen<TestDecision>(_ => true);

            dispatcher.RemoveAllSubscriptions();

            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(0));
            Assert.That(dispatcher.GetDecisionSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void GetTotalSubscriptionCount_SumsBothPaths() {
            var dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestMessage>(_ => { });
            dispatcher.Subscribe<TestMessage>(_ => { });
            dispatcher.Listen<TestDecision>(_ => true);
            dispatcher.Listen<TestDecision>(_ => true);
            dispatcher.Listen<TestDecision>(_ => true);

            Assert.That(dispatcher.GetTotalSubscriptionCount(), Is.EqualTo(5));

            dispatcher.Destroy();
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
        }

        private sealed class CountingMessageHandler
        {
            public int Count { get; private set; }

            public object LastValue { get; private set; }

            public void OnMessage(TestMessage message) {
                Count++;
                LastValue = message.Value;
            }
        }

        private sealed class SubscriptionOwner : BaseObject
        {
            public CountingMessageHandler Listener { get; } = new CountingMessageHandler();

            public void Bind(EventDispatcher dispatcher) {
                var subscription = dispatcher.Subscribe<TestMessage>(Listener.OnMessage, this);
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

            public void Bind(EventDispatcher dispatcher) {
                dispatcher.Subscribe<TestMessage>(Listener.OnMessage, _group);
            }

            protected override void OnCreate() {
                _group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() {
            }
        }
    }
}
