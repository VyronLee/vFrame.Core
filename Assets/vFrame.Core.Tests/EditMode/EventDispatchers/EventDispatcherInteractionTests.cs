using NUnit.Framework;
using vFrame.Core.Base;
using vFrame.Core.EventDispatchers;
using vFrame.Core.Exceptions;

namespace vFrame.Core.Tests.EditMode.EventDispatchers
{
    public class EventDispatcherInteractionTests
    {
        private const int EventId = 101;
        private const int VoteId = 202;

        [Test]
        public void DispatchEvent_InvokesCurrentListeners_AndSkipsRemovedListenerOnNextDispatch() {
            var dispatcher = CreateDispatcher();
            var first = new CountingEventListener();
            var second = new CountingEventListener();

            var firstHandle = dispatcher.AddEventListener(first, EventId);
            dispatcher.AddEventListener(second, EventId);

            dispatcher.DispatchEvent(EventId, "first");
            var removed = dispatcher.RemoveEventListener(firstHandle);
            dispatcher.DispatchEvent(EventId, "second");

            Assert.That(removed, Is.SameAs(first));
            Assert.That(first.Count, Is.EqualTo(1));
            Assert.That(second.Count, Is.EqualTo(2));
            Assert.That(second.LastContext, Is.EqualTo("second"));

            dispatcher.Destroy();
        }

        [Test]
        public void DispatchEvent_AllowsBareSubscriptionToRemainActive_UntilExplicitRemoval() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner(dispatcher);
            var bare = new CountingEventListener();

            owner.Create();
            var bareHandle = dispatcher.AddEventListener(bare, EventId);
            owner.Bind(EventId);

            owner.Destroy();
            dispatcher.DispatchEvent(EventId, 7);
            dispatcher.RemoveEventListener(bareHandle);
            dispatcher.DispatchEvent(EventId, 8);

            Assert.That(owner.Listener.Count, Is.EqualTo(0));
            Assert.That(bare.Count, Is.EqualTo(1));
            Assert.That(bare.LastContext, Is.EqualTo(7));
            Assert.That(dispatcher.GetEventExecutorCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void DispatchEvent_RetainsCompatibilityPath_WhileTypedMessagesStaySeparate() {
            var dispatcher = CreateDispatcher();
            var numeric = new CountingEventListener();
            var typedCount = 0;

            dispatcher.AddEventListener(numeric, EventId);
            dispatcher.Subscribe<CompatibilityMessage>(_ => typedCount++);

            dispatcher.DispatchEvent(EventId, "legacy");
            dispatcher.Publish(new CompatibilityMessage());

            Assert.That(numeric.Count, Is.EqualTo(1));
            Assert.That(numeric.LastContext, Is.EqualTo("legacy"));
            Assert.That(typedCount, Is.EqualTo(1));
            Assert.That(dispatcher.GetEventExecutorCount(), Is.EqualTo(1));
            Assert.That(dispatcher.GetInteractionSubscriptionCount(), Is.EqualTo(1));

            dispatcher.Destroy();
        }

        [Test]
        public void GetDiagnostics_ReflectsCurrentObservableCounts() {
            var dispatcher = CreateDispatcher();
            var numeric = new CountingEventListener();

            dispatcher.AddEventListener(numeric, EventId);
            var subscription = dispatcher.Subscribe<CompatibilityMessage>(_ => { });
            dispatcher.AddVoteListener(new CountingVoteListener(true), VoteId);

            var diagnostics = dispatcher.GetDiagnostics();

            Assert.That(diagnostics.EventExecutorCount, Is.EqualTo(1));
            Assert.That(diagnostics.InteractionSubscriptionCount, Is.EqualTo(1));
            Assert.That(diagnostics.VoteExecutorCount, Is.EqualTo(1));

            dispatcher.Unsubscribe(subscription);
            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_RemovesOwnerBoundSubscription_WhenOwnerLifetimeEnds() {
            var dispatcher = CreateDispatcher();
            var owner = new SubscriptionOwner(dispatcher);

            owner.Create();
            owner.Bind(EventId);
            dispatcher.DispatchEvent(EventId);

            owner.Destroy();
            dispatcher.DispatchEvent(EventId);

            Assert.That(owner.Listener.Count, Is.EqualTo(1));
            Assert.That(dispatcher.GetEventExecutorCount(), Is.EqualTo(0));

            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_RemovesLifetimeBoundSubscription_WhenGroupedLifetimeEnds() {
            var dispatcher = CreateDispatcher();
            var owner = new LifetimeOwnedSubscriptionOwner(dispatcher);

            owner.Create();
            owner.Bind(EventId);
            dispatcher.DispatchEvent(EventId, "before");

            owner.EndGroup();
            dispatcher.DispatchEvent(EventId, "after");

            Assert.That(owner.Listener.Count, Is.EqualTo(1));
            Assert.That(owner.Listener.LastContext, Is.EqualTo("before"));
            Assert.That(dispatcher.GetEventExecutorCount(), Is.EqualTo(0));

            owner.Destroy();
            dispatcher.Destroy();
        }

        [Test]
        public void Destroy_ClearsOwnedState_AndPreventsFurtherUse() {
            var dispatcher = CreateDispatcher();
            var typedCount = 0;

            dispatcher.AddEventListener(new CountingEventListener(), EventId);
            dispatcher.AddVoteListener(new CountingVoteListener(true), VoteId);
            dispatcher.Subscribe<CompatibilityMessage>(_ => typedCount++);

            dispatcher.Destroy();

            Assert.That(dispatcher.Destroyed, Is.True);
            Assert.That(() => dispatcher.Publish(new CompatibilityMessage()), Throws.TypeOf<BaseObjectDestroyedException>());
            Assert.That(() => dispatcher.DispatchEvent(EventId), Throws.TypeOf<BaseObjectDestroyedException>());
            Assert.That(() => dispatcher.DispatchVote(VoteId), Throws.TypeOf<BaseObjectDestroyedException>());
            Assert.That(typedCount, Is.EqualTo(0));
        }

        [Test]
        public void DispatchVote_StopsAfterFirstRejectingListener() {
            var dispatcher = CreateDispatcher();
            var approving = new CountingVoteListener(true);
            var rejecting = new CountingVoteListener(false);
            var trailing = new CountingVoteListener(true);

            dispatcher.AddVoteListener(approving, VoteId);
            dispatcher.AddVoteListener(rejecting, VoteId);
            dispatcher.AddVoteListener(trailing, VoteId);

            var passed = dispatcher.DispatchVote(VoteId, "decision");

            Assert.That(passed, Is.False);
            Assert.That(approving.Count, Is.EqualTo(1));
            Assert.That(rejecting.Count, Is.EqualTo(1));
            Assert.That(trailing.Count, Is.EqualTo(0));
            Assert.That(rejecting.LastContext, Is.EqualTo("decision"));

            dispatcher.Destroy();
        }

        [Test]
        public void DispatchVote_ReturnsTrue_WhenNoVoteListenersAreRegistered() {
            var dispatcher = CreateDispatcher();

            var passed = dispatcher.DispatchVote(VoteId, "none");

            Assert.That(passed, Is.True);

            dispatcher.Destroy();
        }

        [Test]
        public void RemoveEventListener_ClearsPooledDelegateBeforeReuse() {
            var dispatcher = CreateDispatcher();
            var firstCount = 0;
            var secondCount = 0;

            var firstHandle = dispatcher.AddEventListener(_ => firstCount++, EventId);
            dispatcher.RemoveEventListener(firstHandle);

            dispatcher.AddEventListener(_ => secondCount++, EventId);
            dispatcher.DispatchEvent(EventId, "reused");

            Assert.That(firstCount, Is.EqualTo(0));
            Assert.That(secondCount, Is.EqualTo(1));

            dispatcher.Destroy();
        }

        [Test]
        public void RemoveVoteListener_ClearsPooledDelegateBeforeReuse() {
            var dispatcher = CreateDispatcher();
            var firstCount = 0;
            var secondCount = 0;

            var firstHandle = dispatcher.AddVoteListener(_ => {
                firstCount++;
                return true;
            }, VoteId);
            dispatcher.RemoveVoteListener(firstHandle);

            dispatcher.AddVoteListener(_ => {
                secondCount++;
                return true;
            }, VoteId);

            var passed = dispatcher.DispatchVote(VoteId, "reused");

            Assert.That(passed, Is.True);
            Assert.That(firstCount, Is.EqualTo(0));
            Assert.That(secondCount, Is.EqualTo(1));

            dispatcher.Destroy();
        }

        private static EventDispatcher CreateDispatcher() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class CountingEventListener : IEventListener
        {
            public int Count { get; private set; }

            public object LastContext { get; private set; }

            public void OnEvent(IEvent e) {
                Count++;
                LastContext = e.GetContext();
            }
        }

        private sealed class CountingVoteListener : IVoteListener
        {
            private readonly bool _result;

            public CountingVoteListener(bool result) {
                _result = result;
            }

            public int Count { get; private set; }

            public object LastContext { get; private set; }

            public bool OnVote(IVote e) {
                Count++;
                LastContext = e.GetContext();
                return _result;
            }
        }

        private sealed class CompatibilityMessage
        {
        }

        private sealed class SubscriptionOwner : BaseObject
        {
            private readonly EventDispatcher _dispatcher;

            public SubscriptionOwner(EventDispatcher dispatcher) {
                _dispatcher = dispatcher;
            }

            public CountingEventListener Listener { get; } = new CountingEventListener();

            public void Bind(int eventId) {
                var handle = _dispatcher.AddEventListener(Listener, eventId);
                Own(() => _dispatcher.RemoveEventListener(handle));
            }

            protected override void OnCreate() {
            }

            protected override void OnDestroy() { }
        }

        private sealed class LifetimeOwnedSubscriptionOwner : BaseObject
        {
            private readonly EventDispatcher _dispatcher;
            private ILifetime _group;

            public LifetimeOwnedSubscriptionOwner(EventDispatcher dispatcher) {
                _dispatcher = dispatcher;
            }

            public CountingEventListener Listener { get; } = new CountingEventListener();

            public void EndGroup() {
                _group.Destroy();
            }

            public void Bind(int eventId) {
                var handle = _dispatcher.AddEventListener(Listener, eventId);
                _group.Add(() => _dispatcher.RemoveEventListener(handle));
            }

            protected override void OnCreate() {
                _group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() { }
        }
    }
}
