using vFrame.Core.ObjectPools;

namespace vFrame.Core.Generic
{
    public class BoxAllocator<T> : IPoolObjectAllocator<Box<T>>
    {
        public Box<T> Alloc() {
            return new Box<T>();
        }

        public void Reset(Box<T> obj) {
            obj.Value = default;
        }
    }
}