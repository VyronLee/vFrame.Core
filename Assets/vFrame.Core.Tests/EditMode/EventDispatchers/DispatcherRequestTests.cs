using System;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    public class DispatcherRequestTests
    {
        [Test]
        public void Request_WithHandler_ReturnsResponse() {
            var dispatcher = CreateDispatcher();

            dispatcher.HandleRequest<TestRequest, int>(req => req.Value * 2);
            var result = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 21 });

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void Request_WithNoHandler_ReturnsDefault() {
            var dispatcher = CreateDispatcher();

            var result = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 21 });

            Assert.That(result, Is.EqualTo(default(int)));
        }

        [Test]
        public void TryRequest_WithHandler_ReturnsTrue() {
            var dispatcher = CreateDispatcher();

            dispatcher.HandleRequest<TestRequest, int>(req => req.Value * 2);
            var success = dispatcher.TryRequest<TestRequest, int>(new TestRequest { Value = 5 }, out var result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void TryRequest_WithNoHandler_ReturnsFalse() {
            var dispatcher = CreateDispatcher();

            var success = dispatcher.TryRequest<TestRequest, int>(new TestRequest { Value = 5 }, out var result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(default(int)));
        }

        [Test]
        public void HandleRequest_ReturnsSubscription() {
            var dispatcher = CreateDispatcher();

            var subscription = dispatcher.HandleRequest<TestRequest, int>(req => req.Value);

            Assert.That(subscription, Is.Not.Null);
            Assert.That(subscription.Handle, Is.GreaterThan(0));
        }

        [Test]
        public void UnhandleRequest_RemovesHandler() {
            var dispatcher = CreateDispatcher();

            var subscription = dispatcher.HandleRequest<TestRequest, int>(req => req.Value);
            var result1 = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 10 });
            dispatcher.UnhandleRequest(subscription);
            var result2 = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 20 });

            Assert.That(result1, Is.EqualTo(10));
            Assert.That(result2, Is.EqualTo(default(int)));
            Assert.That(dispatcher.GetRequestSubscriptionCount(), Is.EqualTo(0));
        }

        [Test]
        public void UnhandleRequest_OnDestroyedSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();

            var subscription = dispatcher.HandleRequest<TestRequest, int>(req => req.Value);
            dispatcher.UnhandleRequest(subscription);

            Assert.DoesNotThrow(() => dispatcher.UnhandleRequest(subscription));
        }

        [Test]
        public void SecondHandler_ReplacesFirst() {
            var dispatcher = CreateDispatcher();

            dispatcher.HandleRequest<TestRequest, int>(req => 100);
            dispatcher.HandleRequest<TestRequest, int>(req => 200);

            var result = dispatcher.Request<TestRequest, int>(new TestRequest());

            Assert.That(result, Is.EqualTo(200));
            Assert.That(dispatcher.GetRequestSubscriptionCount(), Is.EqualTo(1));
        }

        [Test]
        public void OwnerDestroy_RemovesHandler() {
            var dispatcher = CreateDispatcher();
            var owner = new RequestOwner();
            owner.Create();

            dispatcher.HandleRequest<TestRequest, int>(owner.OnRequest, owner);
            var result1 = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 10 });
            owner.Destroy();
            var result2 = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 20 });

            Assert.That(result1, Is.EqualTo(10));
            Assert.That(result2, Is.EqualTo(default(int)));
        }

        [Test]
        public void LifetimeDestroy_RemovesHandler() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeRequestOwner();
            owner.Create();

            dispatcher.HandleRequest<TestRequest, int>(owner.OnRequest, owner.Group);
            var result1 = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 10 });
            owner.Group.Destroy();
            var success = dispatcher.TryRequest<TestRequest, int>(new TestRequest { Value = 20 }, out var result2);

            Assert.That(result1, Is.EqualTo(10));
            Assert.That(success, Is.False);
        }

        [Test]
        public void Request_ExceptionIsolation_ReturnsDefault() {
            var dispatcher = CreateDispatcher();

            dispatcher.HandleRequest<TestRequest, int>(_ => throw new Exception("test"));

            var result = dispatcher.Request<TestRequest, int>(new TestRequest());

            Assert.That(result, Is.EqualTo(default(int)));
        }

        [Test]
        public void TryRequest_ExceptionIsolation_ReturnsFalse() {
            var dispatcher = CreateDispatcher();

            dispatcher.HandleRequest<TestRequest, int>(_ => throw new Exception("test"));

            var success = dispatcher.TryRequest<TestRequest, int>(new TestRequest(), out var result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(default(int)));
        }

        [Test]
        public void Request_AfterHandlerDestroyed_ReturnsDefault() {
            var dispatcher = CreateDispatcher();

            var sub = dispatcher.HandleRequest<TestRequest, int>(req => req.Value);
            dispatcher.UnhandleRequest(sub);

            var result = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 99 });

            Assert.That(result, Is.EqualTo(default(int)));
        }

        [Test]
        public void Request_DistinctFromEventCommandDecision() {
            var dispatcher = CreateDispatcher();
            var eventCount = 0;
            var commandCount = 0;

            dispatcher.Subscribe<TestEvent>(_ => eventCount++);
            dispatcher.Handle<TestCommand>(_ => commandCount++);
            dispatcher.HandleRequest<TestRequest, int>(req => req.Value);
            dispatcher.Listen<TestDecision>(_ => true);

            dispatcher.Publish(new TestEvent());
            dispatcher.Send(new TestCommand());
            var result = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 42 });
            dispatcher.Decide(new TestDecision());

            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(commandCount, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void Destroy_ClearsRequestSubscriptions() {
            var dispatcher = CreateDispatcher();

            dispatcher.HandleRequest<TestRequest, int>(req => req.Value);
            dispatcher.Destroy();

            Assert.That(dispatcher.Destroyed, Is.True);
        }

        private static Dispatcher CreateDispatcher() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class TestRequest : IRequest<int>
        {
            public int Value { get; set; }
        }

        private sealed class TestEvent : IEvent
        {
        }

        private sealed class TestCommand : ICommand
        {
        }

        private sealed class TestDecision : IDecision
        {
        }

        private sealed class RequestOwner : BaseObject
        {
            public int OnRequest(TestRequest req) {
                return req.Value;
            }

            protected override void OnCreate() {
            }

            protected override void OnDestroy() {
            }
        }

        private sealed class LifetimeRequestOwner : BaseObject
        {
            public ILifetime Group { get; private set; }

            public int OnRequest(TestRequest req) {
                return req.Value;
            }

            protected override void OnCreate() {
                Group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() {
            }
        }
    }
}