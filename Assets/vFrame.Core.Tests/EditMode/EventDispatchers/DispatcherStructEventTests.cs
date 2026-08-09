// ------------------------------------------------------------
//         File: DispatcherStructEventTests.cs
//        Brief: Characterizes the zero-box struct-event hot path
//               of Dispatcher.Publish<TEvent>(in TEvent) (C14):
//               value-type events are delivered by ref and do not
//               box on the dispatch path without interceptors.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-08-09 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Dispatchers
{
    public class DispatcherStructEventTests
    {
        // C14: a value-type event round-trips its exact value through the generic
        // Publish<TEvent>(in TEvent) path. If Publish boxed the payload it would still
        // deliver, so the zero-box guarantee is asserted structurally in the next test.
        [Test]
        public void Publish_StructEvent_RoundTripsValue() {
            var dispatcher = CreateDispatcher();
            var received = 0;

            dispatcher.Subscribe<IntPayloadEvent>(e => received = e.Value);
            dispatcher.Publish(new IntPayloadEvent { Value = 42 });

            Assert.That(received, Is.EqualTo(42));
            Assert.That(dispatcher.GetEventSubscriptionCount(), Is.EqualTo(1));

            dispatcher.Destroy();
        }

        // C14: the struct-event dispatch hot path triggers no GC collections in steady
        // state — the payload is passed by ref (in TEvent) and delivered typed, so no
        // boxing occurs while no interceptors are registered.
        [Test]
        public void Publish_StructEvent_NoGcInSteadyState() {
            var dispatcher = CreateDispatcher();
            var count = 0;

            dispatcher.Subscribe<IntPayloadEvent>(e => count += e.Value);

            var message = new IntPayloadEvent { Value = 1 };

            // Warm up: let any one-time allocations (subscription storage growth) settle.
            for (var i = 0; i < 16; i++) {
                dispatcher.Publish(message);
            }

            var gcBefore = System.GC.CollectionCount(0);
            for (var i = 0; i < 1000; i++) {
                dispatcher.Publish(message);
            }
            var gcAfter = System.GC.CollectionCount(0);

            Assert.That(count, Is.EqualTo(1016),
                "every published struct event must reach the subscriber");
            Assert.That(gcAfter, Is.EqualTo(gcBefore),
                "struct-event Publish hot path must not trigger GC collections (no boxing)");

            dispatcher.Destroy();
        }

        // Distinct struct event types are routed independently by their generic type argument.
        [Test]
        public void Publish_MultipleStructEventTypes_RoutedByType() {
            var dispatcher = CreateDispatcher();
            var ints = 0;
            var longs = 0L;

            dispatcher.Subscribe<IntPayloadEvent>(e => ints += e.Value);
            dispatcher.Subscribe<LongPayloadEvent>(e => longs += e.Value);

            dispatcher.Publish(new IntPayloadEvent { Value = 5 });
            dispatcher.Publish(new LongPayloadEvent { Value = 100 });

            Assert.That(ints, Is.EqualTo(5));
            Assert.That(longs, Is.EqualTo(100));

            dispatcher.Destroy();
        }

        private static Dispatcher CreateDispatcher() {
            var dispatcher = new Dispatcher();
            dispatcher.Create();
            return dispatcher;
        }

        // Plain mutable struct: value-type semantics (not boxed on the 'in' path) are
        // independent of readonly-ness, and mutable fields allow object-initializer syntax.
        private struct IntPayloadEvent : IEvent
        {
            public int Value;
        }

        private struct LongPayloadEvent : IEvent
        {
            public long Value;
        }
    }
}
