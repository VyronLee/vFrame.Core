using NUnit.Framework;
using vFrame.Core.Base;
using vFrame.Core.EventDispatchers;

namespace vFrame.Core.Tests.EditMode.EventDispatchers
{
    public class EventDispatcherDecisionTests
    {
        [Test]
        public void DispatchDecision_UsesVoteSemanticAliasAndShortCircuitsOnRejection() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();

            var firstListener = new CountingVoteListener(true);
            var secondListener = new CountingVoteListener(false);
            var thirdListener = new CountingVoteListener(true);

            dispatcher.AddDecisionListener(firstListener, 1001);
            dispatcher.AddDecisionListener(secondListener, 1001);
            dispatcher.AddDecisionListener(thirdListener, 1001);

            var passed = dispatcher.DispatchDecision(1001, "context");

            Assert.That(passed, Is.False);
            Assert.That(firstListener.CallCount, Is.EqualTo(1));
            Assert.That(secondListener.CallCount, Is.EqualTo(1));
            Assert.That(thirdListener.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void AddDecisionListener_ReturnsHandleRemovableThroughDecisionAlias() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();

            var listener = new CountingVoteListener(true);
            var handle = dispatcher.AddDecisionListener(listener, 1002);

            var removed = dispatcher.RemoveDecisionListener(handle);
            var passed = dispatcher.DispatchDecision(1002);

            Assert.That(removed, Is.SameAs(listener));
            Assert.That(passed, Is.True);
            Assert.That(listener.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void DispatchDecision_RemainsDistinctFromTypedMessageDispatch() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();

            var decisionListener = new CountingVoteListener(true);
            var typedCount = 0;

            dispatcher.AddDecisionListener(decisionListener, 1003);
            dispatcher.Subscribe<DecisionMessage>(_ => typedCount++);

            var passed = dispatcher.DispatchDecision(1003, "decision");
            dispatcher.Publish(new DecisionMessage());

            Assert.That(passed, Is.True);
            Assert.That(decisionListener.CallCount, Is.EqualTo(1));
            Assert.That(typedCount, Is.EqualTo(1));
        }

        [Test]
        public void Destroy_ClearsDecisionListeners() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();

            dispatcher.AddDecisionListener(new CountingVoteListener(true), 1004);

            dispatcher.Destroy();

            Assert.That(() => dispatcher.DispatchDecision(1004), Throws.TypeOf<BaseObjectDestroyedException>());
        }

        private sealed class CountingVoteListener : IVoteListener
        {
            private readonly bool _result;

            public CountingVoteListener(bool result) {
                _result = result;
            }

            public int CallCount { get; private set; }

            public bool OnVote(IVote e) {
                CallCount++;
                return _result;
            }
        }

        private sealed class DecisionMessage
        {
        }
    }
}
