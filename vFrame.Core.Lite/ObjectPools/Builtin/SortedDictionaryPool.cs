using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class SortedDictionaryPool<T1, T2> : ObjectPool<SortedDictionary<T1, T2>, SortedDictionaryAllocator<T1, T2>>
    {
    }
}