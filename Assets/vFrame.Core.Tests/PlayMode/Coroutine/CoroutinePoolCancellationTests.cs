using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using vFrame.Core.Unity.Coroutine;

namespace vFrame.Core.Tests.PlayMode.Coroutine
{
    public class CoroutinePoolCancellationTests
    {
        [UnityTest]
        public IEnumerator StopCoroutine_WhenTaskIsWaiting_DoesNotThrow() {
            var pool = new CoroutinePool("TestPool", 0);

            try {
                var handle = pool.StartCoroutine(DummyTask());

                Assert.DoesNotThrow(() => pool.StopCoroutine(handle));
            }
            finally {
                pool.Destroy();
            }

            yield return null;
        }

        private static IEnumerator DummyTask() {
            yield return null;
        }
    }
}
