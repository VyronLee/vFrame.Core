using vFrame.Core.EventDispatchers;

namespace vFrame.Core.Benchmarks.Editor
{
    [UnityEditor.InitializeOnLoad]
    public static class InteractionDispatchBenchmarks
    {
        private const int DispatchIterations = 100000;
        private const int EventId = 1001;
        private const int ListenerCount = 8;
        private const int MessageSeed = 77;
        private const int VoteId = 2002;

        static InteractionDispatchBenchmarks() {
            CoreBenchmarkRunner.Register(RunEventDispatchBenchmark);
            CoreBenchmarkRunner.Register(RunTypedDispatchBenchmark);
            CoreBenchmarkRunner.Register(RunVoteDispatchBenchmark);
        }

        private static CoreBenchmarkRunner.BenchmarkResult RunEventDispatchBenchmark() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();

            var state = new CounterState();
            for (var i = 0; i < ListenerCount; i++) {
                dispatcher.AddEventListener(state.OnEvent, EventId);
            }

            var result = CoreBenchmarkRunner.Measure(
                "interaction.dispatch.int-event",
                256,
                DispatchIterations,
                () => dispatcher.DispatchEvent(EventId, state));

            dispatcher.Destroy();
            return result;
        }

        private static CoreBenchmarkRunner.BenchmarkResult RunVoteDispatchBenchmark() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();

            for (var i = 0; i < ListenerCount; i++) {
                dispatcher.AddVoteListener(static _ => true, VoteId);
            }

            var result = CoreBenchmarkRunner.Measure(
                "interaction.dispatch.vote",
                256,
                DispatchIterations,
                () => dispatcher.DispatchVote(VoteId, null));

            dispatcher.Destroy();
            return result;
        }

        private static CoreBenchmarkRunner.BenchmarkResult RunTypedDispatchBenchmark() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();

            var state = new TypedCounterState();
            for (var i = 0; i < ListenerCount; i++) {
                dispatcher.Subscribe<TypedMessage>(state.OnMessage);
            }

            var message = new TypedMessage {
                Value = MessageSeed
            };

            var result = CoreBenchmarkRunner.Measure(
                "interaction.dispatch.typed-message",
                256,
                DispatchIterations,
                () => dispatcher.Publish(message));

            dispatcher.Destroy();
            return result;
        }

        private sealed class CounterState
        {
            public int Count;

            public void OnEvent(IEvent _) {
                Count++;
            }
        }

        private sealed class TypedCounterState
        {
            public int Count;

            public void OnMessage(TypedMessage message) {
                Count += message.Value;
            }
        }

        private sealed class TypedMessage
        {
            public int Value;
        }
    }
}
