using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class HashSetPool<T> : ObjectPool<HashSet<T>, HashSetAllocator<T>> { }
}