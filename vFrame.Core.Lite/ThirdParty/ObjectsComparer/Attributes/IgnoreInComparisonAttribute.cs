// ***********************************************************************
// Assembly         : Objects Comparer
// Author           : Ankur Kumar Gupta
// Created          : 24-Feb-2022
// ***********************************************************************

using System;

namespace vFrame.Core.ThirdParty.ObjectsComparer.Attributes
{
  /// <summary>
  /// Class is used to specify whether the element on which it is applied will have comparison effect
  /// </summary>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class IgnoreInComparisonAttribute : Attribute
  {

  }
}
