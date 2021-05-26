using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class SortedDictionaryAllocator<T1, T2> : IPoolObjectAllocator<SortedDictionary<T1, T2>>
    {
        public SortedDictionary<T1, T2> Alloc() {
            return new SortedDictionary<T1, T2>();
        }

        public void Reset(SortedDictionary<T1, T2> obj) {
            obj.Clear();
        }
    }
}