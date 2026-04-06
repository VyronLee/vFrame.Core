using NUnit.Framework;
using vFrame.Core.Base;
using vFrame.Core.EventDispatchers;
using vFrame.Core.Exceptions;

namespace vFrame.Core.Tests.EditMode.EventDispatchers
{
    public class EventDispatcherTypedInteractionTests
    {
        [Test]
        public void Publish_InvokesTypedSubscribers() {
            var dispatcher = CreateDispatcher();
            var received = 0;

            dispatcher.Subscribe<TestMessage>(message => received = message.Value);
            dispatcher.Publish(new TestMessage { Value = 7 });

            Assert.That(received, Is.EqualTo(7));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(1));
        }

        [Test]
        public void OwnerDestroy_RemovesOwnerBoundTypedSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner();
            owner.Create();

            dispatcher.Subscribe<TestMessage>(owner.OnMessage, owner);
            dispatcher.Publish(new TestMessage { Value = 1 });
            owner.Destroy();
            dispatcher.Publish(new TestMessage { Value = 2 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void LifetimeDestroy_RemovesLifetimeBoundTypedSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new GroupOwner();
            owner.Create();

            dispatcher.Subscribe<TestMessage>(owner.OnMessage, owner.Group);
            dispatcher.Publish(new TestMessage { Value = 1 });
            owner.Group.Destroy();
            dispatcher.Publish(new TestMessage { Value = 2 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void BareTypedSubscription_RemainsUntilExplicitUnsubscribe() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            var subscription = dispatcher.Subscribe<TestMessage>(_ => count++);
            dispatcher.Publish(new TestMessage());
            dispatcher.Unsubscribe(subscription);
            dispatcher.Publish(new TestMessage());

            Assert.That(count, Is.EqualTo(1));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void BareTypedSubscription_RemainsActive_AfterBoundSubscriptionsEnd_UntilExplicitUnsubscribe() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner();
            var groupOwner = new GroupOwner();
            var bareCount = 0;

            owner.Create();
            groupOwner.Create();

            var bare = dispatcher.Subscribe<TestMessage>(_ => bareCount++);
            dispatcher.Subscribe<TestMessage>(owner.OnMessage, owner);
            dispatcher.Subscribe<TestMessage>(groupOwner.OnMessage, groupOwner.Group);

            dispatcher.Publish(new TestMessage { Value = 1 });

            owner.Destroy();
            groupOwner.Group.Destroy();
            dispatcher.Publish(new TestMessage { Value = 2 });

            dispatcher.Unsubscribe(bare);
            dispatcher.Publish(new TestMessage { Value = 3 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(groupOwner.Count, Is.EqualTo(1));
            Assert.That(bareCount, Is.EqualTo(2));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void Destroy_RemovesBareTypedSubscription() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            dispatcher.Subscribe<TestMessage>(_ => count++);
            dispatcher.Destroy();

            Assert.That(dispatcher.Destroyed, Is.True);
            Assert.That(count, Is.EqualTo(0));
            Assert.That(() => dispatcher.GetInteractionSubscriptionCount(), Throws.TypeOf<BaseObjectDestroyedException>());
        }

        private static EventDispatcher CreateDispatcher() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class TestMessage
        {
            public int Value { get; set; }
        }

        private sealed class SubscriptionOwner : BaseObject
        {
            public int Count { get; private set; }

            public void OnMessage(TestMessage message) {
                Count += message.Value;
            }

            protected override void OnCreate() {
            }

            protected override void OnDestroy() {
            }
        }

        private sealed class GroupOwner : BaseObject
        {
            public int Count { get; private set; }

            public ILifetime Group { get; private set; }

            public void OnMessage(TestMessage message) {
                Count += message.Value;
            }

            protected override void OnCreate() {
                Group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() {
            }
        }
    }
}
