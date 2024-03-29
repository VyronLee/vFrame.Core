using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class DictionaryAllocator<T1, T2> : IPoolObjectAllocator<Dictionary<T1, T2>>
    {
        public static int PresetLength = 64;

        public Dictionary<T1, T2> Alloc() {
            return new Dictionary<T1, T2>(PresetLength);
        }

        public void Reset(Dictionary<T1, T2> obj) {
            obj.Clear();
        }
    }
}