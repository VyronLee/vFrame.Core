using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.ObjectPools
{
    public class ConcurrentObjectPoolCountTests
    {
        private class RejectPolicy : IPooledObjectPolicy<string>
        {
            public string Create() => "item";
            public bool Return(string obj) => false; // force the destroy-on-return path
        }

        [Test]
        public void Return_RejectedByPolicy_DecrementsCountAll() {
            var pool = new ConcurrentObjectPool<string>(new RejectPolicy());
            var item = pool.Get(); // countAll = 1, active = 1

            pool.Return(item); // rejected -> destroyed

            var stats = pool.GetStatistics();
            Assert.That(stats.CountAll, Is.EqualTo(0),
                "CountAll must drop to 0 after the active object is destroyed");
            Assert.That(stats.CountActive, Is.EqualTo(0),
                "CountActive must be 0 after destroy");
        }
    }
}
