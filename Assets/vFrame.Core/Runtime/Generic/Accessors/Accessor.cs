using System;
using System.Linq.Expressions;
using System.Reflection;

namespace vFrame.Core.Generic
{
    public class Accessor<TSource>
    {
        public static Accessor<TSource, TArg> Create<TArg>(Expression<Func<TSource, TArg>> memberSelector) {
            return new GetterSetter<TArg>(memberSelector);
        }

        public Accessor<TSource, TArg> Get<TArg>(Expression<Func<TSource, TArg>> memberSelector) {
            return Create(memberSelector);
        }

        private class GetterSetter<TArg> : Accessor<TSource, TArg>
        {
            public GetterSetter(Expression<Func<TSource, TArg>> memberSelector) : base(memberSelector) { }
        }
    }

    public class Accessor<TSource, TArg> : Accessor<TSource>
    {
        public readonly Func<TSource, TArg> Getter;
        public readonly Action<TSource, TArg> Setter;

        protected Accessor(Expression<Func<TSource, TArg>> memberSelector) //access not given to outside world
        {
            var prop = memberSelector.GetPropertyInfo();
            IsReadable = prop.CanRead;
            IsWritable = prop.CanWrite;
            AssignDelegate(IsReadable, ref Getter, prop.GetGetMethod());
            AssignDelegate(IsWritable, ref Setter, prop.GetSetMethod());
        }

        public bool IsReadable { get; }
        public bool IsWritable { get; }

        public TArg this[TSource instance] {
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

        private void AssignDelegate<TDelegate>(bool assignable, ref TDelegate assignee, MethodInfo assignor) where TDelegate : class {
            if (assignable) {
                assignee = assignor.CreateDelegate<TDelegate>();
            }
        }
    }

    internal static class ExpressionExtension
    {
        public static Func<TSource, TArg> BuildGetAccessor<TSource, TArg>(Expression<Func<TSource, TArg>> propertySelector) {
            return propertySelector.GetPropertyInfo().GetGetMethod().CreateDelegate<Func<TSource, TArg>>();
        }

        public static Action<TSource, TArg> BuildSetAccessor<TSource, TArg>(Expression<Func<TSource, TArg>> propertySelector) {
            return propertySelector.GetPropertyInfo().GetSetMethod().CreateDelegate<Action<TSource, TArg>>();
        }

        // a generic extension for CreateDelegate
        public static TDelegate CreateDelegate<TDelegate>(this MethodInfo method) where TDelegate : class {
            return Delegate.CreateDelegate(typeof(TDelegate), method) as TDelegate;
        }

        public static PropertyInfo GetPropertyInfo<TSource, TArg>(this Expression<Func<TSource, TArg>> propertySelector) {
            if (!(propertySelector.Body is MemberExpression body)) {
                throw new MissingMemberException();
            }
            return body.Member as PropertyInfo;
        }
    }
}