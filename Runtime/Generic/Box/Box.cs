using vFrame.Core.Base;

namespace vFrame.Core.Generic
{
    public class Box<T> : BaseObject<T>
    {
        public T Value;

        protected override void OnCreate(T value) {
            Value = value;
        }

        protected override void OnDestroy() {
            Value = default;
        }
    }
}