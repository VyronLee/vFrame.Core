// ------------------------------------------------------------
//         File: DispatcherMissingTests.cs
//        Brief: Tests for previously uncovered overloads and edge cases
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-14
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    /// <summary>
    ///     Covers overloads, edge cases, and behaviours not tested by the
    ///     existing Dispatcher*Tests files.
    /// </summary>
    public class DispatcherMissingTests
    {
        // ===================================================================
        //  Marker types (reused across tests)
        // ===================================================================

        private sealed class TestEvent : IEvent
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

        private sealed class TestDecision : IDecision
        {
            public int Value { get; set; }
        }

        // ===================================================================
        //  Helpers
        // ===================================================================

        private static Dispatcher CreateDispatcher() {
            var d = new Dispatcher();
            d.Create();
            return d;
        }

        private sealed class Owner : BaseObject
        {
            public int Count { get; private set; }

            public void OnEvent(TestEvent e) { Count += e.Value; }
            public void OnCommand(TestCommand c) { Count += c.Value; }
            public int OnRequest(TestRequest r) { return r.Value; }
            public bool OnDecision(TestDecision d) { Count += d.Value; return true; }

            protected override void OnCreate() { }
            protected override void OnDestroy() { }
        }

        private sealed class LifetimeOwner : BaseObject
        {
            public ILifetime Group { get; private set; }
            public int Count { get; private set; }

            public void OnEvent(TestEvent e) { Count += e.Value; }
            public void OnCommand(TestCommand c) { Count += c.Value; }
            public int OnRequest(TestRequest r) { return r.Value; }
            public bool OnDecision(TestDecision d) { Count += d.Value; return true; }

            protected override void OnCreate() { Group = Lifetime.CreateChild(); }
            protected override void OnDestroy() { }
        }

        private class StubInterceptor : IEventInterceptor
        {
            public bool CanPublish { get; set; } = true;
            public int BeforeCallCount { get; private set; }
            public int AfterCallCount { get; private set; }
            public int LastSubscriberCount { get; private set; }
            public Type LastEventType { get; private set; }

            public bool OnBeforePublish(Type eventType, ref IEvent eventData) {
                BeforeCallCount++;
                LastEventType = eventType;
                return CanPublish;
            }

            public void OnAfterPublish(Type eventType, IEvent eventData, int subscriberCount) {
                AfterCallCount++;
                LastSubscriberCount = subscriberCount;
                LastEventType = eventType;
            }
        }

        // ===================================================================
        //  1. Subscribe<TEvent>(action, priority, ILifetime)
        // ===================================================================

        [Test]
        public void Subscribe_WithPriorityAndLifetime_BoundAndOrdered() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeOwner();
            owner.Create();
            var order = new List<int>();

            dispatcher.Subscribe<TestEvent>(_ => order.Add(99), priority: -1);
            dispatcher.Subscribe<TestEvent>(_ => order.Add(1), priority: 10, owner.Group);
            dispatcher.Subscribe<TestEvent>(_ => order.Add(2), priority: 0);

            // First publish: priority order 10, 0, -1
            dispatcher.Publish(new TestEvent());
            Assert.That(order, Is.EqualTo(new[] { 1, 2, 99 }));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(3));

            owner.Group.Destroy();
            order.Clear();

            // After lifetime end: priority-10 subscription removed, only priority 0 and -1 remain
            dispatcher.Publish(new TestEvent());
            Assert.That(order, Is.EqualTo(new[] { 2, 99 }));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(2));

            owner.Destroy();
            dispatcher.Destroy();
        }

        // ===================================================================
        //  2. Listen<TDecision>(handler, priority, ILifetime)
        // ===================================================================

        [Test]
        public void Listen_WithPriorityAndLifetime_BoundAndOrdered() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeOwner();
            owner.Create();
            var order = new List<int>();

            dispatcher.Listen<TestDecision>(_ => { order.Add(1); return true; }, priority: 10, owner.Group);
            dispatcher.Listen<TestDecision>(_ => { order.Add(2); return true; }, priority: 0);

            // First decide: priority 10 then 0
            var pass1 = dispatcher.Decide(new TestDecision());
            Assert.That(pass1, Is.True);
            Assert.That(order, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(dispatcher.GetDecisionSubscriptionCount(), Is.EqualTo(2));

            owner.Group.Destroy();
            order.Clear();

            // After lifetime end: only priority 0 remains
            var pass2 = dispatcher.Decide(new TestDecision());
            Assert.That(pass2, Is.True);
            Assert.That(order, Is.EqualTo(new[] { 2 }));
            Assert.That(dispatcher.GetDecisionSubscriptionCount(), Is.EqualTo(1));

            owner.Destroy();
            dispatcher.Destroy();
        }

        // ===================================================================
        //  3. RemoveAllSubscriptions clears all four channels
        // ===================================================================

        [Test]
        public void RemoveAllSubscriptions_ClearsAllFourChannels() {
            var dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Handle<TestCommand>(_ => { });
            dispatcher.HandleRequest<TestRequest, int>(_ => 0);
            dispatcher.Listen<TestDecision>(_ => true);

            Assert.That(dispatcher.GetTotalSubscriptionCount(), Is.EqualTo(4));

            dispatcher.RemoveAllSubscriptions();

            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(0));
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(0));
            Assert.That(dispatcher.GetRequestSubscriptionCount(), Is.EqualTo(0));
            Assert.That(dispatcher.GetDecisionSubscriptionCount(), Is.EqualTo(0));
            Assert.That(dispatcher.GetTotalSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  4. GetTotalSubscriptionCount across all four channels
        // ===================================================================

        [Test]
        public void GetTotalSubscriptionCount_SumsAllFourChannels() {
            var dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Handle<TestCommand>(_ => { });
            dispatcher.HandleRequest<TestRequest, int>(_ => 0);
            dispatcher.Listen<TestDecision>(_ => true);
            dispatcher.Listen<TestDecision>(_ => true);
            dispatcher.Listen<TestDecision>(_ => true);

            // 2 events + 1 command + 1 request + 3 decisions = 7
            Assert.That(dispatcher.GetTotalSubscriptionCount(), Is.EqualTo(7));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  5. GetDiagnostics across all four channels
        // ===================================================================

        [Test]
        public void GetDiagnostics_ReflectsAllFourChannels() {
            var dispatcher = CreateDispatcher();

            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Handle<TestCommand>(_ => { });
            dispatcher.HandleRequest<TestRequest, int>(_ => 0);
            dispatcher.Listen<TestDecision>(_ => true);

            var diag = dispatcher.GetDiagnostics();

            Assert.That(diag.EventSubscriptionCount, Is.EqualTo(2));
            Assert.That(diag.CommandSubscriptionCount, Is.EqualTo(1));
            Assert.That(diag.RequestSubscriptionCount, Is.EqualTo(1));
            Assert.That(diag.DecisionSubscriptionCount, Is.EqualTo(1));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  6. Null subscription handling on Un* methods
        // ===================================================================

        [Test]
        public void Unsubscribe_NullSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();
            Assert.DoesNotThrow(() => dispatcher.Unsubscribe(null));
            dispatcher.Destroy();
        }

        [Test]
        public void Unhandle_NullSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();
            Assert.DoesNotThrow(() => dispatcher.Unhandle(null));
            dispatcher.Destroy();
        }

        [Test]
        public void UnhandleRequest_NullSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();
            Assert.DoesNotThrow(() => dispatcher.UnhandleRequest(null));
            dispatcher.Destroy();
        }

        [Test]
        public void Unlisten_NullSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();
            Assert.DoesNotThrow(() => dispatcher.Unlisten(null));
            dispatcher.Destroy();
        }

        // ===================================================================
        //  7. Multiple interceptors: pipeline order and mid-chain cancel
        // ===================================================================

        [Test]
        public void MultipleInterceptors_InvokedInRegistrationOrder() {
            var dispatcher = CreateDispatcher();
            var beforeOrder = new List<int>();
            var afterOrder = new List<int>();

            dispatcher.AddInterceptor(new SplitInterceptor(1, beforeOrder, afterOrder));
            dispatcher.AddInterceptor(new SplitInterceptor(2, beforeOrder, afterOrder));
            dispatcher.AddInterceptor(new SplitInterceptor(3, beforeOrder, afterOrder));

            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Publish(new TestEvent());

            Assert.That(beforeOrder, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(afterOrder, Is.EqualTo(new[] { 1, 2, 3 }));

            dispatcher.Destroy();
        }

        [Test]
        public void MultipleInterceptors_SecondCancels_StopsPublish() {
            var dispatcher = CreateDispatcher();
            var beforeOrder = new List<int>();
            var afterOrder = new List<int>();
            var handlerCalled = false;

            dispatcher.AddInterceptor(new SplitInterceptor(1, beforeOrder, afterOrder, true));
            dispatcher.AddInterceptor(new SplitInterceptor(2, beforeOrder, afterOrder, false)); // cancels
            dispatcher.AddInterceptor(new SplitInterceptor(3, beforeOrder, afterOrder, true));  // never reached

            dispatcher.Subscribe<TestEvent>(_ => handlerCalled = true);
            dispatcher.Publish(new TestEvent());

            Assert.That(beforeOrder, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(afterOrder, Is.Empty, "OnAfterPublish should not be called when cancelled");
            Assert.That(handlerCalled, Is.False);

            dispatcher.Destroy();
        }

        // ===================================================================
        //  8. RemoveInterceptor return values
        // ===================================================================

        [Test]
        public void RemoveInterceptor_Found_ReturnsTrue() {
            var dispatcher = CreateDispatcher();
            var interceptor = new StubInterceptor();

            dispatcher.AddInterceptor(interceptor);
            var result = dispatcher.RemoveInterceptor(interceptor);

            Assert.That(result, Is.True);
            Assert.That(dispatcher.GetInterceptorCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void RemoveInterceptor_NotFound_ReturnsFalse() {
            var dispatcher = CreateDispatcher();
            var interceptor = new StubInterceptor();

            var result = dispatcher.RemoveInterceptor(interceptor);

            Assert.That(result, Is.False);

            dispatcher.Destroy();
        }

        [Test]
        public void RemoveInterceptor_Null_ReturnsFalse() {
            var dispatcher = CreateDispatcher();

            var result = dispatcher.RemoveInterceptor(null);

            Assert.That(result, Is.False);

            dispatcher.Destroy();
        }

        [Test]
        public void RemoveInterceptor_AlreadyRemoved_ReturnsFalse() {
            var dispatcher = CreateDispatcher();
            var interceptor = new StubInterceptor();

            dispatcher.AddInterceptor(interceptor);
            dispatcher.RemoveInterceptor(interceptor);
            var result = dispatcher.RemoveInterceptor(interceptor);

            Assert.That(result, Is.False);

            dispatcher.Destroy();
        }

        // ===================================================================
        //  9. HandleRequest with RegisterMode.Ignore returns null
        // ===================================================================

        [Test]
        public void HandleRequest_IgnoreMode_ReturnsNullOnDuplicate() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.HandleRequest<TestRequest, int>(r => 42);
            var second = dispatcher.HandleRequest<TestRequest, int>(r => 99, RegisterMode.Ignore);

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Null);

            // Original handler still active
            var result = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 1 });
            Assert.That(result, Is.EqualTo(42));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  10. OnAfterPublish subscriber count when no subscribers
        // ===================================================================

        [Test]
        public void Interceptor_OnAfterPublish_NotCalledWhenNoSubscribers() {
            var dispatcher = CreateDispatcher();
            var interceptor = new StubInterceptor { CanPublish = true };

            dispatcher.AddInterceptor(interceptor);
            dispatcher.Publish(new TestEvent()); // no subscribers for this event type

            // OnBeforePublish is called but Publish short-circuits before OnAfterPublish
            Assert.That(interceptor.BeforeCallCount, Is.EqualTo(1));
            Assert.That(interceptor.AfterCallCount, Is.EqualTo(0));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  11. Subscription Dispose() method
        // ===================================================================

        [Test]
        public void Subscription_Dispose_MarksAsDestroyed() {
            var dispatcher = CreateDispatcher();
            var subscription = dispatcher.Subscribe<TestEvent>(_ => { });

            Assert.That(subscription.Destroyed, Is.False);
            subscription.Dispose();
            Assert.That(subscription.Destroyed, Is.True);

            dispatcher.Destroy();
        }

        // ===================================================================
        //  12. Handle replaces already-destroyed handler (Command)
        // ===================================================================

        [Test]
        public void Handle_ReplacingDestroyedHandler_RegistersNew() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.Handle<TestCommand>(c => { });
            dispatcher.Unhandle(first);
            // first is destroyed but still in the dictionary until Send cleans it up

            var secondCalled = false;
            var second = dispatcher.Handle<TestCommand>(c => secondCalled = true);

            Assert.That(second, Is.Not.Null);
            dispatcher.Send(new TestCommand());
            Assert.That(secondCalled, Is.True);
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(1));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  13. HandleRequest replaces already-destroyed handler (Request)
        // ===================================================================

        [Test]
        public void HandleRequest_ReplacingDestroyedHandler_RegistersNew() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.HandleRequest<TestRequest, int>(r => 0);
            dispatcher.UnhandleRequest(first);

            var second = dispatcher.HandleRequest<TestRequest, int>(r => r.Value * 3);

            Assert.That(second, Is.Not.Null);
            var result = dispatcher.Request<TestRequest, int>(new TestRequest { Value = 7 });
            Assert.That(result, Is.EqualTo(21));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  14. Decision: exception in handler does not veto (swallowed)
        // ===================================================================

        [Test]
        public void Decide_ExceptionInHandler_DoesNotVeto() {
            var dispatcher = CreateDispatcher();

            // Handler throws instead of returning false (veto)
            // The exception is swallowed, so pass remains true
            dispatcher.Listen<TestDecision>(_ => throw new Exception("boom"));

            var pass = dispatcher.Decide(new TestDecision());

            // Exception is caught; handler did not explicitly return false,
            // so pass stays true (AND logic — no explicit false encountered)
            Assert.That(pass, Is.True);

            dispatcher.Destroy();
        }

        [Test]
        public void Decide_ThrowingHandlerFollowedByVeto_ReturnsFalse() {
            var dispatcher = CreateDispatcher();
            var vetoReached = false;

            dispatcher.Listen<TestDecision>(_ => throw new Exception("boom"));
            dispatcher.Listen<TestDecision>(_ => { vetoReached = true; return false; });

            var pass = dispatcher.Decide(new TestDecision());

            // Throwing handler does not short-circuit; second handler is reached
            Assert.That(vetoReached, Is.True);
            Assert.That(pass, Is.False);

            dispatcher.Destroy();
        }

        // ===================================================================
        //  15. Double Un* is safe (already-destroyed subscription)
        // ===================================================================

        [Test]
        public void Unsubscribe_AlreadyDestroyedSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();
            var sub = dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Unsubscribe(sub);

            Assert.DoesNotThrow(() => dispatcher.Unsubscribe(sub));

            dispatcher.Destroy();
        }

        [Test]
        public void Unlisten_AlreadyDestroyedSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();
            var sub = dispatcher.Listen<TestDecision>(_ => true);
            dispatcher.Unlisten(sub);

            Assert.DoesNotThrow(() => dispatcher.Unlisten(sub));

            dispatcher.Destroy();
        }

        [Test]
        public void UnhandleRequest_AlreadyDestroyedSubscription_IsNoOp() {
            var dispatcher = CreateDispatcher();
            var sub = dispatcher.HandleRequest<TestRequest, int>(_ => 0);
            dispatcher.UnhandleRequest(sub);

            Assert.DoesNotThrow(() => dispatcher.UnhandleRequest(sub));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  16. Command count after re-registration
        // ===================================================================

        [Test]
        public void GetCommandSubscriptionCount_AfterReplacement_StillOne() {
            var dispatcher = CreateDispatcher();

            dispatcher.Handle<TestCommand>(_ => { });
            dispatcher.Handle<TestCommand>(_ => { }); // replaces

            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(1));

            dispatcher.Destroy();
        }

        [Test]
        public void GetRequestSubscriptionCount_AfterReplacement_StillOne() {
            var dispatcher = CreateDispatcher();

            dispatcher.HandleRequest<TestRequest, int>(_ => 1);
            dispatcher.HandleRequest<TestRequest, int>(_ => 2); // replaces

            Assert.That(dispatcher.GetRequestSubscriptionCount(), Is.EqualTo(1));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  17. Publish with no subscribers does not throw
        // ===================================================================

        [Test]
        public void Publish_NoSubscribers_DoesNotThrow() {
            var dispatcher = CreateDispatcher();

            Assert.DoesNotThrow(() => dispatcher.Publish(new TestEvent()));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  18. Multiple event types are independent
        // ===================================================================

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

        private sealed class OtherEvent : IEvent
        {
        }

        // ===================================================================
        //  19. Subscription Handle is unique and monotonically increasing
        // ===================================================================

        [Test]
        public void Subscription_Handles_AreUniqueAndIncreasing() {
            var dispatcher = CreateDispatcher();

            var sub1 = dispatcher.Subscribe<TestEvent>(_ => { });
            var sub2 = dispatcher.Subscribe<TestEvent>(_ => { });
            var sub3 = dispatcher.Handle<TestCommand>(_ => { });

            Assert.That(sub2.Handle, Is.GreaterThan(sub1.Handle));
            Assert.That(sub3.Handle, Is.GreaterThan(sub2.Handle));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  20. Owner-bound subscription for Command and Request
        // ===================================================================

        [Test]
        public void Handle_OwnerBoundSubscription_RemovedOnOwnerDestroy() {
            var dispatcher = CreateDispatcher();
            var owner = new Owner();
            owner.Create();

            dispatcher.Handle<TestCommand>(owner.OnCommand, owner);
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(1));

            owner.Destroy();
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void HandleRequest_OwnerBoundSubscription_RemovedOnOwnerDestroy() {
            var dispatcher = CreateDispatcher();
            var owner = new Owner();
            owner.Create();

            dispatcher.HandleRequest<TestRequest, int>(owner.OnRequest, owner);
            Assert.That(dispatcher.GetRequestSubscriptionCount(), Is.EqualTo(1));

            owner.Destroy();
            Assert.That(dispatcher.GetRequestSubscriptionCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  21. Lifetime-bound subscription for Command and Request
        // ===================================================================

        [Test]
        public void Handle_LifetimeBoundSubscription_RemovedOnLifetimeEnd() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeOwner();
            owner.Create();

            dispatcher.Handle<TestCommand>(owner.OnCommand, owner.Group);
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(1));

            owner.Group.Destroy();
            Assert.That(dispatcher.GetCommandSubscriptionCount(), Is.EqualTo(0));

            owner.Destroy();
            dispatcher.Destroy();
        }

        [Test]
        public void HandleRequest_LifetimeBoundSubscription_RemovedOnLifetimeEnd() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeOwner();
            owner.Create();

            dispatcher.HandleRequest<TestRequest, int>(owner.OnRequest, owner.Group);
            Assert.That(dispatcher.GetRequestSubscriptionCount(), Is.EqualTo(1));

            owner.Group.Destroy();
            Assert.That(dispatcher.GetRequestSubscriptionCount(), Is.EqualTo(0));

            owner.Destroy();
            dispatcher.Destroy();
        }

        // ===================================================================
        //  22. Interceptor OnBeforePublish receives correct event type
        // ===================================================================

        [Test]
        public void Interceptor_ReceivesCorrectEventType() {
            var dispatcher = CreateDispatcher();
            var interceptor = new StubInterceptor { CanPublish = true };

            dispatcher.AddInterceptor(interceptor);
            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Publish(new TestEvent());

            Assert.That(interceptor.LastEventType, Is.EqualTo(typeof(TestEvent)));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  23. DiagnosticsSnapshot is a readonly struct with expected values
        // ===================================================================

        [Test]
        public void DiagnosticsSnapshot_StoresCorrectValues() {
            var snapshot = new Dispatcher.DiagnosticsSnapshot(
                eventCount: 10,
                commandCount: 20,
                requestCount: 30,
                decisionCount: 40);

            Assert.That(snapshot.EventSubscriptionCount, Is.EqualTo(10));
            Assert.That(snapshot.CommandSubscriptionCount, Is.EqualTo(20));
            Assert.That(snapshot.RequestSubscriptionCount, Is.EqualTo(30));
            Assert.That(snapshot.DecisionSubscriptionCount, Is.EqualTo(40));
        }

        // ===================================================================
        //  24. Multiple subscriptions for the same event type
        // ===================================================================

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

        // ===================================================================
        //  25. Decision with multiple listeners: all approve
        // ===================================================================

        [Test]
        public void Decide_MultipleListenersAllApprove_ReturnsTrue() {
            var dispatcher = CreateDispatcher();
            var callCount = 0;

            dispatcher.Listen<TestDecision>(_ => { callCount++; return true; });
            dispatcher.Listen<TestDecision>(_ => { callCount++; return true; });
            dispatcher.Listen<TestDecision>(_ => { callCount++; return true; });

            var pass = dispatcher.Decide(new TestDecision());

            Assert.That(pass, Is.True);
            Assert.That(callCount, Is.EqualTo(3));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  26. Decision priority: higher priority checked first, veto stops chain
        // ===================================================================

        [Test]
        public void Decide_PriorityVeto_StopsChain() {
            var dispatcher = CreateDispatcher();
            var order = new List<int>();

            // Priority 0 (low) — approved, should run
            dispatcher.Listen<TestDecision>(_ => { order.Add(3); return true; }, priority: 0);
            // Priority 10 (high) — veto, should run first and stop chain
            dispatcher.Listen<TestDecision>(_ => { order.Add(1); return false; }, priority: 10);
            // Priority 5 (medium) — approved, should NOT be reached due to veto
            dispatcher.Listen<TestDecision>(_ => { order.Add(2); return true; }, priority: 5);

            var pass = dispatcher.Decide(new TestDecision());

            Assert.That(pass, Is.False);
            Assert.That(order, Is.EqualTo(new[] { 1 }));
            // Only the high-priority veto was invoked; lower priorities never reached

            dispatcher.Destroy();
        }

        // ===================================================================
        //  27. AddInterceptor after Destroy throws
        // ===================================================================

        [Test]
        public void AddInterceptor_AfterDestroy_ThrowsBaseObjectDestroyedException() {
            var dispatcher = CreateDispatcher();
            dispatcher.Destroy();

            Assert.Throws<BaseObjectDestroyedException>(() =>
                dispatcher.AddInterceptor(new StubInterceptor()));
        }

        // ===================================================================
        //  28. Subscription Priority default is zero for bare Subscribe
        // ===================================================================

        [Test]
        public void Subscribe_DefaultPriority_IsZero() {
            var dispatcher = CreateDispatcher();

            var sub = dispatcher.Subscribe<TestEvent>(_ => { });
            Assert.That(sub.Priority, Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void Listen_DefaultPriority_IsZero() {
            var dispatcher = CreateDispatcher();

            var sub = dispatcher.Listen<TestDecision>(_ => true);
            Assert.That(sub.Priority, Is.EqualTo(0));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  29. Post-publish interceptors still run after successful publish
        // ===================================================================

        [Test]
        public void MultipleInterceptors_AllOnAfterPublishCalled() {
            var dispatcher = CreateDispatcher();
            var afterCount = new List<int>();

            var a = new AfterCountingInterceptor(1, afterCount);
            var b = new AfterCountingInterceptor(2, afterCount);

            dispatcher.AddInterceptor(a);
            dispatcher.AddInterceptor(b);
            dispatcher.Subscribe<TestEvent>(_ => { });

            dispatcher.Publish(new TestEvent());

            Assert.That(afterCount, Is.EqualTo(new[] { 1, 2 }));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  30. RemoveAllSubscriptions does not affect interceptor pipeline
        // ===================================================================

        [Test]
        public void RemoveAllSubscriptions_DoesNotRemoveInterceptors() {
            var dispatcher = CreateDispatcher();
            var interceptor = new StubInterceptor { CanPublish = true };

            dispatcher.AddInterceptor(interceptor);
            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.RemoveAllSubscriptions();

            Assert.That(dispatcher.GetInterceptorCount(), Is.EqualTo(1));

            // Interceptor still receives callbacks
            dispatcher.Subscribe<TestEvent>(_ => { });
            dispatcher.Publish(new TestEvent());
            Assert.That(interceptor.AfterCallCount, Is.EqualTo(1));

            dispatcher.Destroy();
        }

        // ===================================================================
        //  Interceptor helpers
        // ===================================================================

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
