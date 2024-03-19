using System;
using System.Collections.Generic;
using vFrame.Core.ThirdParty.ObjectsComparer;
using vFrame.Core.ThirdParty.ObjectsCopy;

namespace vFrame.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsPrimitive(this Type type) {
            if (type == typeof(string)) {
                return true;
            }
            return type.IsValueType & type.IsPrimitive;
        }

        public static T DeepCopy<T>(this T original) {
            return (T)ObjectCopy.DeepCopy(original);
        }

        public static T DeepCopyFrom<T>(this T original, T target) {
            return (T)ObjectCopy.DeepCopyFrom(original, target);
        }

        public static bool DeepCompare<T>(this T original, T target) {
            var comparer = new ThirdParty.ObjectsComparer.Comparer<T>();
            return comparer.Compare(original, target);
        }

        public static bool DeepCompare<T>(this T original, T target, IComparersFactory factory) {
            var comparer = new ThirdParty.ObjectsComparer.Comparer<T>(factory: factory);
            return comparer.Compare(original, target);
        }

        public static bool DeepCompare<T>(this T original, T target, out IEnumerable<Difference> differences) {
            var comparer = new ThirdParty.ObjectsComparer.Comparer<T>();
            return comparer.Compare(original, target, out differences);
        }

        public static bool DeepCompare<T>(this T original, T target, IComparersFactory factory,
            out IEnumerable<Difference> differences) {
            var comparer = new ThirdParty.ObjectsComparer.Comparer<T>(factory: factory);
            return comparer.Compare(original, target, out differences);
        }
    }
}