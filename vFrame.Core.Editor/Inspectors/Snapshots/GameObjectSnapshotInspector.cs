using System;
using UnityEditor;
using UnityEngine;

namespace vFrame.Core.Inspectors.Snapshots
{
    public class GameObjectSnapshotInspector : Editor
    {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}