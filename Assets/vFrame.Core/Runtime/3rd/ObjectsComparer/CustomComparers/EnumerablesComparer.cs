﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using vFrame.Core.ThirdParty.ObjectsComparer.Utils;

namespace vFrame.Core.ThirdParty.ObjectsComparer.CustomComparers
{
    internal class EnumerablesComparer : AbstractComparer, IComparerWithCondition
    {
        public EnumerablesComparer(ComparisonSettings settings, BaseComparer parentComparer, IComparersFactory factory)
            : base(settings, parentComparer, factory)
        {
        }

        public override IEnumerable<Difference> CalculateDifferences(Type type, object obj1, object obj2)
        {
            if (!Settings.EmptyAndNullEnumerablesEqual &&
                (obj1 == null || obj2 == null) && obj1 != obj2)
            {
                yield return new Difference("[]", obj1?.ToString() ?? string.Empty, obj2?.ToString() ?? string.Empty);
                yield break;
            }

            obj1 = obj1 ?? Enumerable.Empty<object>();
            obj2 = obj2 ?? Enumerable.Empty<object>();

            if (!type.InheritsFrom(typeof(IEnumerable)))
            {
                throw new ArgumentException(nameof(type));
            }

            if (!obj1.GetType().InheritsFrom(typeof(IEnumerable)))
            {
                throw new ArgumentException(nameof(obj1));
            }

            if (!obj2.GetType().InheritsFrom(typeof(IEnumerable)))
            {
                throw new ArgumentException(nameof(obj2));
            }

            var array1 = ((IEnumerable)obj1).Cast<object>().ToArray();
            var array2 = ((IEnumerable)obj2).Cast<object>().ToArray();

            if (array1.Length != array2.Length)
            {
                yield return new Difference("", array1.Length.ToString(), array2.Length.ToString(), 
                    DifferenceTypes.NumberOfElementsMismatch);
                yield break;
            }

            //ToDo Extract type
            for (var i = 0; i < array2.Length; i++)
            {
                if (array1[i] == null && array2[i] == null)
                {
                    continue;
                }

                var valueComparer1 = array1[i] != null ? OverridesCollection.GetComparer(array1[i].GetType()) ?? DefaultValueComparer : DefaultValueComparer;
                var valueComparer2 = array2[i] != null ? OverridesCollection.GetComparer(array2[i].GetType()) ?? DefaultValueComparer : DefaultValueComparer;

                if (array1[i] == null)
                {
                    yield return new Difference($"[{i}]", string.Empty, valueComparer2.ToString(array2[i]));
                    continue;
                }

                if (array2[i] == null)
                {
                    yield return new Difference($"[{i}]", valueComparer1.ToString(array1[i]), string.Empty);
                    continue;
                }

                if (array1[i].GetType() != array2[i].GetType())
                {
                    yield return new Difference($"[{i}]", valueComparer1.ToString(array1[i]), valueComparer2.ToString(array2[i]), DifferenceTypes.TypeMismatch);
                    continue;
                }

                var comparer = Factory.GetObjectsComparer(array1[i].GetType(), Settings, this);
                foreach (var failure in comparer.CalculateDifferences(array1[i].GetType(), array1[i], array2[i]))
                {
                    yield return failure.InsertPath($"[{i}]");
                }
            }
        }

        public bool IsMatch(Type type, object obj1, object obj2)
        {
            return type.InheritsFrom(typeof(IEnumerable)) && !type.InheritsFrom(typeof(IEnumerable<>));
        }

        public bool IsStopComparison(Type type, object obj1, object obj2)
        {
            return Settings.EmptyAndNullEnumerablesEqual && obj1 == null || obj2 == null;
        }

        public bool SkipMember(Type type, MemberInfo member)
        {
            return false;
        }
    }
}
