using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using vFrame.Core.Unity;

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

        [UnityTest]
        public IEnumerator Destroy_WhenFixtureTearsDown_CleansUpPoolHolder() {
            var pool = new CoroutinePool("TeardownPool", 1);

            pool.Destroy();

            yield return null;

            var poolHolder = GameObject.Find("Pool_2(TeardownPool)") ?? GameObject.Find("Pool_1(TeardownPool)");
            Assert.That(poolHolder, Is.Null);
        }

        private static IEnumerator DummyTask() {
            yield return null;
        }
    }
}
