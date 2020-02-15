using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class HashSetAllocator<T> : IPoolObjectAllocator<HashSet<T>>
    {
        public HashSet<T> Alloc() {
            return new HashSet<T>();
        }

        public void Reset(HashSet<T> obj) {
            obj.Clear();
        }
    }
}
