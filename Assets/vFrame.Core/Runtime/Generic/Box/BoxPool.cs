using vFrame.Core.ObjectPools;

namespace vFrame.Core.Generic
{
    public class BoxPool<T> : ObjectPool<Box<T>, BoxAllocator<T>> { }
}