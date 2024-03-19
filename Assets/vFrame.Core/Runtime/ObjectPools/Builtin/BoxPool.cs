using vFrame.Core.Base;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class BoxPool<T> : ObjectPool<Box<T>, BoxAllocator<T>> { }
}