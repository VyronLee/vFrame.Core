using UnityEditor;
using UnityEngine;
using vFrame.Core.Behaviours.Snapshots;

namespace vFrame.Core.Inspectors.Snapshots
{
    [CustomEditor(typeof(GameObjectSnapshotBehaviour))]
    public class GameObjectSnapshotBehaviourInspector : Editor
    {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}