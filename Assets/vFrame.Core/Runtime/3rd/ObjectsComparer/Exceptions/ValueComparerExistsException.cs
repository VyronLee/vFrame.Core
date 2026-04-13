using System;
using System.Reflection;

namespace vFrame.Core
{
    /// <summary>
    ///     Represents errors that occur when comparer for Member has already been added.
    /// </summary>
    public class ValueComparerExistsException : Exception
    {
        internal ValueComparerExistsException(MemberInfo memberInfo)
            : base($"Comparer override for member {memberInfo.MemberType} has already been added.") {
            MemberInfo = memberInfo;
        }

        /// <summary>
        ///     MemberInfo of the Member that was a cause of exception
        /// </summary>
        public MemberInfo MemberInfo { get; }
    }
}