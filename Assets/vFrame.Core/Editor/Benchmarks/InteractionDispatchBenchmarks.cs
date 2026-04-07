using vFrame.Core.EventDispatchers;

namespace vFrame.Core.Benchmarks.Editor
{
    [UnityEditor.InitializeOnLoad]
    public static class InteractionDispatchBenchmarks
    {
        private const int DispatchIterations = 100000;
        private const int ListenerCount = 8;
        private const int MessageSeed = 77;

        static InteractionDispatchBenchmarks() {
            CoreBenchmarkRunner.Register(RunTypedDispatchBenchmark);
            CoreBenchmarkRunner.Register(RunDecisionDispatchBenchmark);
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

        private static CoreBenchmarkRunner.BenchmarkResult RunDecisionDispatchBenchmark() {
            var dispatcher = new EventDispatcher();
            dispatcher.Create();

            for (var i = 0; i < ListenerCount; i++) {
                dispatcher.Listen<TypedDecision>(static _ => true);
            }

            var decision = new TypedDecision {
                Value = MessageSeed
            };

            var result = CoreBenchmarkRunner.Measure(
                "interaction.dispatch.typed-decision",
                256,
                DispatchIterations,
                () => dispatcher.Decide(decision));

            dispatcher.Destroy();
            return result;
        }

        private sealed class TypedCounterState
        {
            public int Count;

            public void OnMessage(TypedMessage message) {
                Count += message.Value;
            }
        }

        private sealed class TypedMessage : IInteractionMessage
        {
            public int Value;
        }

        private sealed class TypedDecision : IDecisionMessage
        {
            public int Value;
        }
    }
}
