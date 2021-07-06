using vFrame.Core.Base;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class BoxAllocator<T> : IPoolObjectAllocator<Box<T>>
    {
        public Box<T> Alloc() {
            return new Box<T>();
        }

        public void Reset(Box<T> obj) {

        }
    }
}