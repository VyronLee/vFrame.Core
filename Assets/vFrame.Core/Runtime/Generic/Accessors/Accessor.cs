// ------------------------------------------------------------
//         File: Accessor.cs
//        Brief: Fast property accessors built from expression trees.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-20 16:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace vFrame.Core
{
    public class Accessor<TSource>
    {
        /// <summary>
        ///     Creates a typed accessor for the specified property.
        /// </summary>
        /// <typeparam name="TArg">The property value type.</typeparam>
        /// <param name="memberSelector">An expression selecting the target property.</param>
        /// <returns>An accessor providing fast get/set delegates for the property.</returns>
        public static Accessor<TSource, TArg> Create<TArg>(Expression<Func<TSource, TArg>> memberSelector) {
            return new GetterSetter<TArg>(memberSelector);
        }

        /// <summary>
        ///     Creates a typed accessor for the specified property.
        /// </summary>
        /// <typeparam name="TArg">The property value type.</typeparam>
        /// <param name="memberSelector">An expression selecting the target property.</param>
        /// <returns>An accessor providing fast get/set delegates for the property.</returns>
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

        /// <summary>
        ///     Builds fast get/set delegates from the given property expression.
        /// </summary>
        /// <param name="memberSelector">An expression selecting the target property.</param>
        protected Accessor(Expression<Func<TSource, TArg>> memberSelector)
        {
            var prop = memberSelector.GetPropertyInfo();
            if (prop == null) {
                ThrowHelper.ThrowArgumentException(
                    $"Expression must select a property, not a field: {memberSelector}");
            }
            IsReadable = prop.CanRead;
            IsWritable = prop.CanWrite;
            AssignDelegate(IsReadable, ref Getter, prop.GetGetMethod());
            AssignDelegate(IsWritable, ref Setter, prop.GetSetMethod());
        }

        /// <summary>
        ///     Gets whether the target property can be read.
        /// </summary>
        public bool IsReadable { get; }

        /// <summary>
        ///     Gets whether the target property can be written.
        /// </summary>
        public bool IsWritable { get; }

        /// <summary>
        ///     Gets or sets the property value on the given instance.
        /// </summary>
        /// <param name="instance">The source instance to access.</param>
        /// <exception cref="ArgumentException">Thrown when the property is not readable or not writable.</exception>
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

        /// <summary>
        ///     Assigns a delegate from the given method info when the condition is met.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type to create.</typeparam>
        /// <param name="assignable">Whether the delegate should be assigned.</param>
        /// <param name="assignee">The delegate field to assign.</param>
        /// <param name="assignor">The method info to create the delegate from.</param>
        private void AssignDelegate<TDelegate>(bool assignable, ref TDelegate assignee, MethodInfo assignor) where TDelegate : class {
            if (assignable) {
                assignee = assignor.CreateDelegate<TDelegate>();
            }
        }
    }

    internal static class ExpressionExtension
    {
        /// <summary>
        ///     Builds a fast getter delegate from a property selector expression.
        /// </summary>
        public static Func<TSource, TArg> BuildGetAccessor<TSource, TArg>(Expression<Func<TSource, TArg>> propertySelector) {
            return propertySelector.GetPropertyInfo().GetGetMethod().CreateDelegate<Func<TSource, TArg>>();
        }

        /// <summary>
        ///     Builds a fast setter delegate from a property selector expression.
        /// </summary>
        public static Action<TSource, TArg> BuildSetAccessor<TSource, TArg>(Expression<Func<TSource, TArg>> propertySelector) {
            return propertySelector.GetPropertyInfo().GetSetMethod().CreateDelegate<Action<TSource, TArg>>();
        }

        /// <summary>
        ///     Creates a delegate of the specified type from a method info.
        /// </summary>
        public static TDelegate CreateDelegate<TDelegate>(this MethodInfo method) where TDelegate : class {
            return Delegate.CreateDelegate(typeof(TDelegate), method) as TDelegate;
        }

        /// <summary>
        ///     Extracts property info from a member expression.
        /// </summary>
        /// <exception cref="MissingMemberException">Thrown when the expression body is not a member expression.</exception>
        public static PropertyInfo GetPropertyInfo<TSource, TArg>(this Expression<Func<TSource, TArg>> propertySelector) {
            if (!(propertySelector.Body is MemberExpression body)) {
                throw new MissingMemberException();
            }
            return body.Member as PropertyInfo;
        }
    }
}
