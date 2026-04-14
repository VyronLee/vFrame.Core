using System;
using System.Collections.Generic;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.EventDispatchers
{
    public class DispatcherAdvancedTests
    {
        // --- Marker types ---

        private struct TestEvent : IEvent { public int Value; }

        private struct TestCommand : ICommand { public int Value; }

        private struct TestRequest : IRequest<int> { public int Value; }

        private struct TestDecision : IDecision { public int Threshold; }

        // --- Priority tests ---

        [Test]
        public void Subscribe_WithPriority_InvokesInOrder() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var order = new List<int>();

                dispatcher.Subscribe<TestEvent>(e => order.Add(1), priority: 10);
                dispatcher.Subscribe<TestEvent>(e => order.Add(3), priority: 0);
                dispatcher.Subscribe<TestEvent>(e => order.Add(2), priority: 5);

                dispatcher.Publish(new TestEvent { Value = 42 });

                Assert.That(order, Is.EqualTo(new[] { 1, 2, 3 }));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void Subscribe_SamePriority_InvokesInRegistrationOrder() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var order = new List<int>();

                dispatcher.Subscribe<TestEvent>(e => order.Add(1), priority: 5);
                dispatcher.Subscribe<TestEvent>(e => order.Add(2), priority: 5);
                dispatcher.Subscribe<TestEvent>(e => order.Add(3), priority: 5);

                dispatcher.Publish(new TestEvent { Value = 42 });

                Assert.That(order, Is.EqualTo(new[] { 1, 2, 3 }));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void Subscribe_NegativePriority_InvokesLast() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var order = new List<int>();

                dispatcher.Subscribe<TestEvent>(e => order.Add(1));
                dispatcher.Subscribe<TestEvent>(e => order.Add(2), priority: -10);

                dispatcher.Publish(new TestEvent { Value = 42 });

                Assert.That(order, Is.EqualTo(new[] { 1, 2 }));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        // --- Decision priority tests ---

        [Test]
        public void Listen_WithPriority_InvokesInOrder() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var order = new List<int>();

                dispatcher.Listen<TestDecision>(d => { order.Add(1); return true; }, priority: 10);
                dispatcher.Listen<TestDecision>(d => { order.Add(3); return true; }, priority: 0);
                dispatcher.Listen<TestDecision>(d => { order.Add(2); return true; }, priority: 5);

                dispatcher.Decide(new TestDecision { Threshold = 5 });

                Assert.That(order, Is.EqualTo(new[] { 1, 2, 3 }));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        // --- RegisterMode tests ---

        [Test]
        public void Handle_ThrowMode_ThrowsOnDuplicate() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                dispatcher.Handle<TestCommand>(c => { });
                Assert.Throws<ArgumentException>(() =>
                    dispatcher.Handle<TestCommand>(c => { }, RegisterMode.Throw));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void Handle_IgnoreMode_ReturnsNullOnDuplicate() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var first = dispatcher.Handle<TestCommand>(c => { });
                var second = dispatcher.Handle<TestCommand>(c => { }, RegisterMode.Ignore);

                Assert.That(first, Is.Not.Null);
                Assert.That(second, Is.Null);
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void Handle_IgnoreMode_RetainsOriginalHandler() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var called = false;
                dispatcher.Handle<TestCommand>(c => { called = true; });
                dispatcher.Handle<TestCommand>(c => { called = false; }, RegisterMode.Ignore);

                dispatcher.Send(new TestCommand { Value = 1 });
                Assert.That(called, Is.True, "Original handler should still be active");
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void HandleRequest_ThrowMode_ThrowsOnDuplicate() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                dispatcher.HandleRequest<TestRequest, int>(r => 42);
                Assert.Throws<ArgumentException>(() =>
                    dispatcher.HandleRequest<TestRequest, int>(r => 99, RegisterMode.Throw));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void HandleRequest_IgnoreMode_RetainsOriginalHandler() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                dispatcher.HandleRequest<TestRequest, int>(r => 42);
                dispatcher.HandleRequest<TestRequest, int>(r => 99, RegisterMode.Ignore);

                var result = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 1 });
                Assert.That(result, Is.EqualTo(42), "Original handler should still be active");
            }
            finally {
                dispatcher.Destroy();
            }
        }

        // --- Interceptor tests ---

        [Test]
        public void Interceptor_OnBeforePublish_CanCancel() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var handlerCalled = false;
                var interceptor = new TestEventInterceptor(canPublish: false);

                dispatcher.Subscribe<TestEvent>(e => { handlerCalled = true; });
                dispatcher.AddInterceptor(interceptor);
                dispatcher.Publish(new TestEvent { Value = 1 });

                Assert.That(handlerCalled, Is.False, "Handler should not be called when interceptor cancels");
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void Interceptor_OnBeforePublish_AllowsPublish() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var handlerCalled = false;
                var interceptor = new TestEventInterceptor(canPublish: true);

                dispatcher.Subscribe<TestEvent>(e => { handlerCalled = true; });
                dispatcher.AddInterceptor(interceptor);
                dispatcher.Publish(new TestEvent { Value = 1 });

                Assert.That(handlerCalled, Is.True, "Handler should be called when interceptor allows");
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void Interceptor_OnAfterPublish_ReceivesSubscriberCount() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var interceptor = new TestEventInterceptor(canPublish: true);
                dispatcher.Subscribe<TestEvent>(e => { });
                dispatcher.Subscribe<TestEvent>(e => { });
                dispatcher.AddInterceptor(interceptor);

                dispatcher.Publish(new TestEvent { Value = 1 });

                Assert.That(interceptor.LastSubscriberCount, Is.EqualTo(2));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void RemoveInterceptor_StopsReceivingCallbacks() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var interceptor = new TestEventInterceptor(canPublish: false);
                dispatcher.AddInterceptor(interceptor);
                dispatcher.RemoveInterceptor(interceptor);

                var handlerCalled = false;
                dispatcher.Subscribe<TestEvent>(e => { handlerCalled = true; });
                dispatcher.Publish(new TestEvent { Value = 1 });

                Assert.That(handlerCalled, Is.True, "Handler should be called after interceptor removed");
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void GetInterceptorCount_ReturnsCorrectCount() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                Assert.That(dispatcher.GetInterceptorCount(), Is.EqualTo(0));
                dispatcher.AddInterceptor(new TestEventInterceptor(canPublish: true));
                Assert.That(dispatcher.GetInterceptorCount(), Is.EqualTo(1));
                dispatcher.AddInterceptor(new TestEventInterceptor(canPublish: true));
                Assert.That(dispatcher.GetInterceptorCount(), Is.EqualTo(2));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        // --- DiagnosticsSnapshot ---

        [Test]
        public void GetDiagnostics_ReturnsCorrectCounts() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                dispatcher.Subscribe<TestEvent>(e => { });
                dispatcher.Subscribe<TestEvent>(e => { });
                dispatcher.Handle<TestCommand>(c => { });
                dispatcher.HandleRequest<TestRequest, int>(r => 42);
                dispatcher.Listen<TestDecision>(d => true);

                var diag = dispatcher.GetDiagnostics();

                Assert.That(diag.EventSubscriptionCount, Is.EqualTo(2));
                Assert.That(diag.CommandSubscriptionCount, Is.EqualTo(1));
                Assert.That(diag.RequestSubscriptionCount, Is.EqualTo(1));
                Assert.That(diag.DecisionSubscriptionCount, Is.EqualTo(1));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        // --- ISubscription.Priority ---

        [Test]
        public void Subscription_ReturnsAssignedPriority() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var sub = dispatcher.Subscribe<TestEvent>(e => { }, priority: 42);
                Assert.That(sub.Priority, Is.EqualTo(42));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        // --- Helper classes ---

        private class TestEventInterceptor : IEventInterceptor
        {
            private readonly bool _canPublish;
            public int LastSubscriberCount { get; private set; }
            public int BeforeCallCount { get; private set; }
            public int AfterCallCount { get; private set; }
            public Type LastEventType { get; private set; }

            public TestEventInterceptor(bool canPublish) {
                _canPublish = canPublish;
            }

            public bool OnBeforePublish(Type eventType, ref IEvent eventData) {
                BeforeCallCount++;
                LastEventType = eventType;
                return _canPublish;
            }

            public void OnAfterPublish(Type eventType, IEvent eventData, int subscriberCount) {
                AfterCallCount++;
                LastSubscriberCount = subscriberCount;
                LastEventType = eventType;
            }
        }

        // --- Additional coverage for interceptor pipeline and diagnostics edge cases ---

        [Test]
        public void MultipleInterceptors_InvokedInRegistrationOrder() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var beforeOrder = new List<int>();
                var afterOrder = new List<int>();

                dispatcher.AddInterceptor(new SplitInterceptor(1, beforeOrder, afterOrder));
                dispatcher.AddInterceptor(new SplitInterceptor(2, beforeOrder, afterOrder));
                dispatcher.AddInterceptor(new SplitInterceptor(3, beforeOrder, afterOrder));

                dispatcher.Subscribe<TestEvent>(_ => { });
                dispatcher.Publish(new TestEvent());

                Assert.That(beforeOrder, Is.EqualTo(new[] { 1, 2, 3 }));
                Assert.That(afterOrder, Is.EqualTo(new[] { 1, 2, 3 }));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void MultipleInterceptors_SecondCancels_StopsPublish() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var beforeOrder = new List<int>();
                var afterOrder = new List<int>();
                var handlerCalled = false;

                dispatcher.AddInterceptor(new SplitInterceptor(1, beforeOrder, afterOrder, true));
                dispatcher.AddInterceptor(new SplitInterceptor(2, beforeOrder, afterOrder, false));
                dispatcher.AddInterceptor(new SplitInterceptor(3, beforeOrder, afterOrder, true));

                dispatcher.Subscribe<TestEvent>(_ => handlerCalled = true);
                dispatcher.Publish(new TestEvent());

                Assert.That(beforeOrder, Is.EqualTo(new[] { 1, 2 }));
                Assert.That(afterOrder, Is.Empty, "OnAfterPublish should not be called when cancelled");
                Assert.That(handlerCalled, Is.False);
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void RemoveInterceptor_Found_ReturnsTrue() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var interceptor = new TestEventInterceptor(true);
                dispatcher.AddInterceptor(interceptor);
                var result = dispatcher.RemoveInterceptor(interceptor);

                Assert.That(result, Is.True);
                Assert.That(dispatcher.GetInterceptorCount(), Is.EqualTo(0));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void RemoveInterceptor_NotFound_ReturnsFalse() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var result = dispatcher.RemoveInterceptor(new TestEventInterceptor(true));
                Assert.That(result, Is.False);
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void RemoveInterceptor_Null_ReturnsFalse() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                Assert.That(dispatcher.RemoveInterceptor(null), Is.False);
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void RemoveInterceptor_AlreadyRemoved_ReturnsFalse() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var interceptor = new TestEventInterceptor(true);
                dispatcher.AddInterceptor(interceptor);
                dispatcher.RemoveInterceptor(interceptor);
                Assert.That(dispatcher.RemoveInterceptor(interceptor), Is.False);
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void Interceptor_OnAfterPublish_NotCalledWhenNoSubscribers() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var interceptor = new TestEventInterceptor(true);
                dispatcher.AddInterceptor(interceptor);
                dispatcher.Publish(new TestEvent());

                Assert.That(interceptor.BeforeCallCount, Is.EqualTo(1));
                Assert.That(interceptor.AfterCallCount, Is.EqualTo(0));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void Interceptor_ReceivesCorrectEventType() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var interceptor = new TestEventInterceptor(true);
                dispatcher.AddInterceptor(interceptor);
                dispatcher.Subscribe<TestEvent>(_ => { });
                dispatcher.Publish(new TestEvent());

                Assert.That(interceptor.LastEventType, Is.EqualTo(typeof(TestEvent)));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void DiagnosticsSnapshot_StoresCorrectValues() {
            var snapshot = new Dispatcher.DiagnosticsSnapshot(
                eventCount: 10, commandCount: 20, requestCount: 30, decisionCount: 40);

            Assert.That(snapshot.EventSubscriptionCount, Is.EqualTo(10));
            Assert.That(snapshot.CommandSubscriptionCount, Is.EqualTo(20));
            Assert.That(snapshot.RequestSubscriptionCount, Is.EqualTo(30));
            Assert.That(snapshot.DecisionSubscriptionCount, Is.EqualTo(40));
        }

        [Test]
        public void AddInterceptor_AfterDestroy_ThrowsBaseObjectDestroyedException() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            dispatcher.Destroy();
            Assert.Throws<BaseObjectDestroyedException>(() =>
                dispatcher.AddInterceptor(new TestEventInterceptor(true)));
        }

        [Test]
        public void MultipleInterceptors_AllOnAfterPublishCalled() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var afterCount = new List<int>();

                dispatcher.AddInterceptor(new AfterCountingInterceptor(1, afterCount));
                dispatcher.AddInterceptor(new AfterCountingInterceptor(2, afterCount));
                dispatcher.Subscribe<TestEvent>(_ => { });
                dispatcher.Publish(new TestEvent());

                Assert.That(afterCount, Is.EqualTo(new[] { 1, 2 }));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        [Test]
        public void RemoveAllSubscriptions_DoesNotRemoveInterceptors() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            try {
                var interceptor = new TestEventInterceptor(true);
                dispatcher.AddInterceptor(interceptor);
                dispatcher.Subscribe<TestEvent>(_ => { });
                dispatcher.RemoveAllSubscriptions();

                Assert.That(dispatcher.GetInterceptorCount(), Is.EqualTo(1));

                dispatcher.Subscribe<TestEvent>(_ => { });
                dispatcher.Publish(new TestEvent());
                Assert.That(interceptor.AfterCallCount, Is.EqualTo(1));
            }
            finally {
                dispatcher.Destroy();
            }
        }

        // --- Additional interceptor helpers ---

        private class SplitInterceptor : IEventInterceptor
        {
            private readonly int _id;
            private readonly List<int> _beforeOrder;
            private readonly List<int> _afterOrder;
            private readonly bool _canPublish;

            public SplitInterceptor(int id, List<int> beforeOrder, List<int> afterOrder, bool canPublish = true) {
                _id = id;
                _beforeOrder = beforeOrder;
                _afterOrder = afterOrder;
                _canPublish = canPublish;
            }

            public bool OnBeforePublish(Type eventType, ref IEvent eventData) {
                _beforeOrder.Add(_id);
                return _canPublish;
            }

            public void OnAfterPublish(Type eventType, IEvent eventData, int subscriberCount) {
                _afterOrder.Add(_id);
            }
        }

        private class AfterCountingInterceptor : IEventInterceptor
        {
            private readonly int _id;
            private readonly List<int> _log;

            public AfterCountingInterceptor(int id, List<int> log) {
                _id = id;
                _log = log;
            }

            public bool OnBeforePublish(Type eventType, ref IEvent eventData) => true;

            public void OnAfterPublish(Type eventType, IEvent eventData, int subscriberCount) {
                _log.Add(_id);
            }
        }
    }
}
