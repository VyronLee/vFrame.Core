using System;
using System.Linq.Expressions;
using System.Reflection;

namespace vFrame.Core.Reflections
{
    public class Accessor<S>
    {
        public static Accessor<S, T> Create<T>(Expression<Func<S, T>> memberSelector) {
            return new GetterSetter<T>(memberSelector);
        }

        public Accessor<S, T> Get<T>(Expression<Func<S, T>> memberSelector) {
            return Create(memberSelector);
        }

        private class GetterSetter<T> : Accessor<S, T>
        {
            public GetterSetter(Expression<Func<S, T>> memberSelector) : base(memberSelector) { }
        }
    }

    public class Accessor<S, T> : Accessor<S>
    {
        public readonly Func<S, T> Getter;
        public readonly Action<S, T> Setter;

        protected Accessor(Expression<Func<S, T>> memberSelector) //access not given to outside world
        {
            var prop = memberSelector.GetPropertyInfo();
            IsReadable = prop.CanRead;
            IsWritable = prop.CanWrite;
            AssignDelegate(IsReadable, ref Getter, prop.GetGetMethod());
            AssignDelegate(IsWritable, ref Setter, prop.GetSetMethod());
        }

        public bool IsReadable { get; }
        public bool IsWritable { get; }

        public T this[S instance] {
            get {
                if (!IsReadable) {
                    throw new ArgumentException("Property get method not found.");
                }
                return Getter(instance);
            }
            set {
                if (!IsWritable) {
                    throw new ArgumentException("Property set method not found.");
                }
                Setter(instance, value);
            }
        }

        private void AssignDelegate<K>(bool assignable, ref K assignee, MethodInfo assignor) where K : class {
            if (assignable) {
                assignee = assignor.CreateDelegate<K>();
            }
        }
    }

    internal static class ExpressionExtension
    {
        public static Func<S, T> BuildGetAccessor<S, T>(Expression<Func<S, T>> propertySelector) {
            return propertySelector.GetPropertyInfo().GetGetMethod().CreateDelegate<Func<S, T>>();
        }

        public static Action<S, T> BuildSetAccessor<S, T>(Expression<Func<S, T>> propertySelector) {
            return propertySelector.GetPropertyInfo().GetSetMethod().CreateDelegate<Action<S, T>>();
        }

        // a generic extension for CreateDelegate
        public static T CreateDelegate<T>(this MethodInfo method) where T : class {
            return Delegate.CreateDelegate(typeof(T), method) as T;
        }

        public static PropertyInfo GetPropertyInfo<S, T>(this Expression<Func<S, T>> propertySelector) {
            if (!(propertySelector.Body is MemberExpression body)) {
                throw new MissingMemberException();
            }
            return body.Member as PropertyInfo;
        }
    }
}