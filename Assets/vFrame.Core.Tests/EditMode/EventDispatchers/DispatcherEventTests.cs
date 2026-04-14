using System.Collections.Generic;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    public class DispatcherEventTests
    {
        [Test]
        public void Publish_InvokesTypedSubscribers() {
            var dispatcher = CreateDispatcher();
            var received = 0;

            dispatcher.Subscribe<TestEvent>(message => received = message.Value);
            dispatcher.Publish(new TestEvent { Value = 7 });

            Assert.That(received, Is.EqualTo(7));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(1));
        }

        [Test]
        public void OwnerDestroy_RemovesOwnerBoundSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner();
            owner.Create();

            dispatcher.Subscribe<TestEvent>(owner.OnMessage, owner);
            dispatcher.Publish(new TestEvent { Value = 1 });
            owner.Destroy();
            dispatcher.Publish(new TestEvent { Value = 2 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void LifetimeDestroy_RemovesLifetimeBoundSubscription() {
            var dispatcher = CreateDispatcher();
            var owner = new GroupOwner();
            owner.Create();

            dispatcher.Subscribe<TestEvent>(owner.OnMessage, owner.Group);
            dispatcher.Publish(new TestEvent { Value = 1 });
            owner.Group.Destroy();
            dispatcher.Publish(new TestEvent { Value = 2 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void BareSubscription_RemainsUntilExplicitUnsubscribe() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            var subscription = dispatcher.Subscribe<TestEvent>(_ => count++);
            dispatcher.Publish(new TestEvent());
            dispatcher.Unsubscribe(subscription);
            dispatcher.Publish(new TestEvent());

            Assert.That(count, Is.EqualTo(1));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void BareSubscription_RemainsActive_AfterBoundSubscriptionsEnd() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner();
            var groupOwner = new GroupOwner();
            var bareCount = 0;

            owner.Create();
            groupOwner.Create();

            var bare = dispatcher.Subscribe<TestEvent>(_ => bareCount++);
            dispatcher.Subscribe<TestEvent>(owner.OnMessage, owner);
            dispatcher.Subscribe<TestEvent>(groupOwner.OnMessage, groupOwner.Group);

            dispatcher.Publish(new TestEvent { Value = 1 });

            owner.Destroy();
            groupOwner.Group.Destroy();
            dispatcher.Publish(new TestEvent { Value = 2 });

            dispatcher.Unsubscribe(bare);
            dispatcher.Publish(new TestEvent { Value = 3 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(groupOwner.Count, Is.EqualTo(1));
            Assert.That(bareCount, Is.EqualTo(2));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void Destroy_RemovesBareSubscription() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            dispatcher.Subscribe<TestEvent>(_ => count++);
            dispatcher.Destroy();

            Assert.That(dispatcher.Destroyed, Is.True);
            Assert.That(count, Is.EqualTo(0));
            Assert.That(() => dispatcher.GetEventSubscriptionCount(), Throws.TypeOf<BaseObjectDestroyedException>());
        }

        [Test]
        public void Publish_ExceptionIsolation_InvokesRemainingSubscribers() {
            var dispatcher = CreateDispatcher();
            var secondCalled = false;

            dispatcher.Subscribe<TestEvent>(_ => throw new System.Exception("test"));
            dispatcher.Subscribe<TestEvent>(_ => secondCalled = true);

            dispatcher.Publish(new TestEvent());

            Assert.That(secondCalled, Is.True);
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

        private sealed class SubscriptionOwner : BaseObject
        {
            public int Count { get; private set; }

            public void OnMessage(TestEvent message) {
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

            public void OnMessage(TestEvent message) {
                Count += message.Value;
            }

            protected override void OnCreate() {
                Group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() {
            }
        }

        // --- Additional coverage for untested overloads and edge cases ---

        [Test]
        public void Subscribe_WithPriorityAndLifetime_BoundAndOrdered() {
            var dispatcher = CreateDispatcher();
            var owner = new GroupOwner();
            owner.Create();
            var order = new List<int>();

            dispatcher.Subscribe<TestEvent>(_ => order.Add(99), priority: -1);
            dispatcher.Subscribe<TestEvent>(_ => order.Add(1), priority: 10, owner.Group);
            dispatcher.Subscribe<TestEvent>(_ => order.Add(2), priority: 0);

            dispatcher.Publish(new TestEvent());
            Assert.That(order, Is.EqualTo(new[] { 1, 2, 99 }));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(3));

            owner.Group.Destroy();
            order.Clear();

            dispatcher.Publish(new TestEvent());
            Assert.That(order, Is.EqualTo(new[] { 2, 99 }));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(2));

            owner.Destroy();
            dispatcher.Destroy();
        }

        [Test]
        public void Publish_NoSubscribers_DoesNotThrow() {
            var dispatcher = CreateDispatcher();
            Assert.DoesNotThrow(() => dispatcher.Publish(new TestEvent()));
            dispatcher.Destroy();
        }

        [Test]
        public void Publish_MultipleSubscribers_AllInvoked() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            dispatcher.Subscribe<TestEvent>(_ => count++);
            dispatcher.Subscribe<TestEvent>(_ => count++);
            dispatcher.Subscribe<TestEvent>(_ => count++);

            dispatcher.Publish(new TestEvent());

            Assert.That(count, Is.EqualTo(3));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(3));

            dispatcher.Destroy();
        }

        [Test]
        public void Subscribe_DifferentEventTypes_DoNotInterfere() {
            var dispatcher = CreateDispatcher();
            var countA = 0;
            var countB = 0;

            dispatcher.Subscribe<TestEvent>(_ => countA++);
            dispatcher.Subscribe<OtherEvent>(_ => countB++);
            dispatcher.Publish(new TestEvent());

            Assert.That(countA, Is.EqualTo(1));
            Assert.That(countB, Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void Subscribe_DefaultPriority_IsZero() {
            var dispatcher = CreateDispatcher();
            var sub = dispatcher.Subscribe<TestEvent>(_ => { });
            Assert.That(sub.Priority, Is.EqualTo(0));
            dispatcher.Destroy();
        }

        private sealed class OtherEvent : IEvent
        {
        }
    }
}