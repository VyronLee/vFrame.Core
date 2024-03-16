//------------------------------------------------------------
//        File:  ComponentExtension.cs
//       Brief:  ComponentExtension
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-01-02 21:37
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.Reflection;
using UnityEngine;

namespace vFrame.Core.Extensions.UnityEngine
{
    public static class ComponentExtension
    {
        private const BindingFlags Flags = BindingFlags.Public
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Instance
                                           | BindingFlags.Default
                                           | BindingFlags.DeclaredOnly;

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component {
            var type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            var propertiesInfo = type.GetProperties(Flags);
            foreach (var info in propertiesInfo)
                if (info.CanWrite)
                    try {
                        info.SetValue(comp, info.GetValue(other, null), null);
                    }
                    catch {
                        // In case of NotImplementedException being thrown.
                        // For some reason specifying that exception didn't seem to catch it,
                        // so I didn't catch anything specific.
                    }

            var fieldsInfo = type.GetFields(Flags);
            foreach (var info in fieldsInfo)
                info.SetValue(comp, info.GetValue(other));
            return comp as T;
        }
    }
}