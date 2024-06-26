using System;
using System.Collections.Generic;
using System.Reflection;
using vFrame.Core.ThirdParty.ObjectsComparer.Utils;

namespace vFrame.Core.ThirdParty.ObjectsComparer.CustomComparers
{
    internal abstract class AbstractEnumerablesComparer: AbstractComparer, IComparerWithCondition
    {
        protected AbstractEnumerablesComparer(ComparisonSettings settings, BaseComparer parentComparer,
            IComparersFactory factory)
            : base(settings, parentComparer, factory)
        {
        }

        public bool IsStopComparison(Type type, object obj1, object obj2)
        {
            return Settings.EmptyAndNullEnumerablesEqual && (obj1 == null || obj2 == null);
        }

        public virtual bool SkipMember(Type type, MemberInfo member)
        {
            if (type.InheritsFrom(typeof(Array)))
            {
                if (member.Name == "LongLength")
                {
                    return true;
                }
            }

            if (type.InheritsFrom(typeof(List<>)))
            {
                if (member.Name == PropertyHelper.GetMemberInfo(() => new List<string>().Capacity).Name)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract override IEnumerable<Difference> CalculateDifferences(Type type, object obj1, object obj2);

        public abstract bool IsMatch(Type type, object obj1, object obj2);
    }
}