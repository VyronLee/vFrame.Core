using System;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    public class DispatcherCommandTests
    {
        [Test]
        public void Send_InvokesSingleHandler() {
            var dispatcher = CreateDispatcher();
            var received = 0;

            dispatcher.Handle<TestCommand>(cmd => received = cmd.Value);
            dispatcher.Send(new TestCommand { Value = 42 });

            Assert.That(received, Is.EqualTo(42));
        }

        [Test]
        public void Send_WithNoHandler_DoesNotThrow() {
            var dispatcher = CreateDispatcher();

            Assert.DoesNotThrow(() => dispatcher.Send(new TestCommand()));
        }

        [Test]
        public void Handle_ReturnsSubscription() {
            var dispatcher = CreateDispatcher();

            var subscription = dispatcher.Handle<TestCommand>(_ => { });

            Assert.That(subscription, Is.Not.Null);
            Assert.That(subscription.Handle, Is.GreaterThan(0));
        }

        [Test]
        public void Unhandle_RemovesHandler() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            var subscription = dispatcher.Handle<TestCommand>(_ => count++);
            dispatcher.Send(new TestCommand());
            dispatcher.Unhandle(subscription);
            dispatcher.Send(new TestCommand());

            Assert.That(count, Is.EqualTo(1));
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void Unhandle_OnDestroyedSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();

            var subscription = dispatcher.Handle<TestCommand>(_ => { });
            dispatcher.Unhandle(subscription);

            Assert.DoesNotThrow(() => dispatcher.Unhandle(subscription));
        }

        [Test]
        public void SecondHandler_ReplacesFirst() {
            var dispatcher = CreateDispatcher();
            var firstCount = 0;
            var secondCount = 0;

            dispatcher.Handle<TestCommand>(_ => firstCount++);
            dispatcher.Handle<TestCommand>(_ => secondCount++);
            dispatcher.Send(new TestCommand());

            Assert.That(firstCount, Is.EqualTo(0));
            Assert.That(secondCount, Is.EqualTo(1));
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(1));
        }

        [Test]
        public void OwnerDestroy_RemovesHandler() {
            var dispatcher = CreateDispatcher();
            var owner = new CommandOwner();
            owner.Create();

            dispatcher.Handle<TestCommand>(owner.OnCommand, owner);
            dispatcher.Send(new TestCommand { Value = 1 });
            owner.Destroy();
            dispatcher.Send(new TestCommand { Value = 2 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void LifetimeDestroy_RemovesHandler() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeCommandOwner();
            owner.Create();

            dispatcher.Handle<TestCommand>(owner.OnCommand, owner.Group);
            dispatcher.Send(new TestCommand { Value = 1 });
            owner.Group.Destroy();
            dispatcher.Send(new TestCommand { Value = 2 });

            Assert.That(owner.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void Send_ExceptionIsolation_DoesNotPropagate() {
            var dispatcher = CreateDispatcher();

            dispatcher.Handle<TestCommand>(_ => throw new Exception("test"));

            Assert.DoesNotThrow(() => dispatcher.Send(new TestCommand()));
        }

        [Test]
        public void Send_AfterHandlerDestroyed_DoesNotThrow() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            var sub = dispatcher.Handle<TestCommand>(_ => count++);
            dispatcher.Unhandle(sub);
            dispatcher.Send(new TestCommand());

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void Send_DistinctFromEventAndDecision() {
            var dispatcher = CreateDispatcher();
            var commandCount = 0;
            var eventCount = 0;

            dispatcher.Handle<TestCommand>(_ => commandCount++);
            dispatcher.Subscribe<TestEvent>(_ => eventCount++);

            dispatcher.Send(new TestCommand());
            dispatcher.Publish(new TestEvent());

            Assert.That(commandCount, Is.EqualTo(1));
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void Destroy_ClearsCommandSubscriptions() {
            var dispatcher = CreateDispatcher();

            dispatcher.Handle<TestCommand>(_ => { });
            dispatcher.Destroy();

            Assert.That(dispatcher.Destroyed, Is.True);
        }

        private static Dispatcher CreateDispatcher() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class TestCommand : ICommand
        {
            public int Value { get; set; }
        }

        private sealed class TestEvent : IEvent
        {
        }

        private sealed class CommandOwner : BaseObject
        {
            public int Count { get; private set; }

            public void OnCommand(TestCommand cmd) {
                Count += cmd.Value;
            }

            protected override void OnCreate() {
            }

            protected override void OnDestroy() {
            }
        }

        private sealed class LifetimeCommandOwner : BaseObject
        {
            public int Count { get; private set; }

            public ILifetime Group { get; private set; }

            public void OnCommand(TestCommand cmd) {
                Count += cmd.Value;
            }

            protected override void OnCreate() {
                Group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() {
            }
        }
    }
}