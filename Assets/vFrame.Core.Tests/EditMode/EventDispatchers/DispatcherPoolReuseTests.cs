using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    public class DispatcherPoolReuseTests
    {
        [Test]
        public void EventSubscription_IsReusedAfterUnsubscribeAndPublish() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.Subscribe<TestEvent>(_ => { });
            var firstHandle = first.Handle;
            dispatcher.Unsubscribe(first);
            dispatcher.Publish(new TestEvent());

            var second = dispatcher.Subscribe<TestEvent>(_ => { });
            var secondHandle = second.Handle;

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Destroyed, Is.False);
            Assert.That(secondHandle, Is.Not.EqualTo(firstHandle));

            dispatcher.Destroy();
        }

        [Test]
        public void DecisionSubscription_IsReusedAfterUnlistenAndDecide() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.Listen<TestDecision>(_ => true);
            var firstHandle = first.Handle;
            dispatcher.Unlisten(first);
            dispatcher.Decide(new TestDecision());

            var second = dispatcher.Listen<TestDecision>(_ => true);
            var secondHandle = second.Handle;

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Destroyed, Is.False);
            Assert.That(secondHandle, Is.Not.EqualTo(firstHandle));

            dispatcher.Destroy();
        }

        [Test]
        public void CommandSubscription_IsReusedAfterUnhandleAndSend() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.Handle<TestCommand>(_ => { });
            var firstHandle = first.Handle;
            dispatcher.Unhandle(first);
            dispatcher.Send(new TestCommand());

            var second = dispatcher.Handle<TestCommand>(_ => { });
            var secondHandle = second.Handle;

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Destroyed, Is.False);
            Assert.That(secondHandle, Is.Not.EqualTo(firstHandle));

            dispatcher.Destroy();
        }

        [Test]
        public void RequestSubscription_IsReusedAfterUnhandleRequestAndRequest() {
            var dispatcher = CreateDispatcher();

            var first = dispatcher.HandleRequest<TestRequest, int>(req => req.Value);
            var firstHandle = first.Handle;
            dispatcher.UnhandleRequest(first);
            dispatcher.Request<TestRequest, int>(new TestRequest());

            var second = dispatcher.HandleRequest<TestRequest, int>(req => req.Value);
            var secondHandle = second.Handle;

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Destroyed, Is.False);
            Assert.That(secondHandle, Is.Not.EqualTo(firstHandle));

            dispatcher.Destroy();
        }

        private static Dispatcher CreateDispatcher() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        private sealed class TestEvent : IEvent
        {
        }

        private sealed class TestDecision : IDecision
        {
        }

        private sealed class TestCommand : ICommand
        {
        }

        private sealed class TestRequest : IRequest<int>
        {
            public int Value { get; set; }
        }
    }
}