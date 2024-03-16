//------------------------------------------------------------
//        File:  EditorUtils.cs
//       Brief:  Editor 工具集
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-11-07 20:47
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.IO;
using UnityEditor;

namespace vFrame.Core.Utils
{
    public static class EditorUtils
    {
        public static string GetSelectedPath() {
            var path = string.Empty;

            foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
                path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    continue;

                path = Path.GetDirectoryName(path);
                break;
            }
            return path;
        }
    }
}