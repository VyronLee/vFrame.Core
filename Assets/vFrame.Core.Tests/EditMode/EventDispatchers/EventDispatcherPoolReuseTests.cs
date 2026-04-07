using NUnit.Framework;
using vFrame.Core.EventDispatchers;

namespace vFrame.Core.Tests.EditMode.EventDispatchers
{
    public class EventDispatcherPoolReuseTests
    {
        [Test]
        public void InteractionSubscription_IsReusedAfterUnsubscribeAndPublish() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.Subscribe<TestMessage>(_ => { });
            var firstHandle = first.Handle;
            dispatcher.Unsubscribe(first);
            dispatcher.Publish(new TestMessage());

            var second = dispatcher.Subscribe<TestMessage>(_ => { });
            var secondHandle = second.Handle;

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Destroyed, Is.False);
            Assert.That(secondHandle, Is.Not.EqualTo(firstHandle));

            dispatcher.Destroy();
        }

        [Test]
        public void DecisionSubscription_IsReusedAfterUnsubscribeAndDecide() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.Listen<TestDecision>(_ => true);
            var firstHandle = first.Handle;
            dispatcher.Unsubscribe(first);
            dispatcher.Decide(new TestDecision());

            var second = dispatcher.Listen<TestDecision>(_ => true);
            var secondHandle = second.Handle;

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Destroyed, Is.False);
            Assert.That(secondHandle, Is.Not.EqualTo(firstHandle));

            dispatcher.Destroy();
        }

        private static EventDispatcher CreateDispatcher() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class TestMessage : IInteractionMessage
        {
        }

        private sealed class TestDecision : IDecisionMessage
        {
        }
    }
}
