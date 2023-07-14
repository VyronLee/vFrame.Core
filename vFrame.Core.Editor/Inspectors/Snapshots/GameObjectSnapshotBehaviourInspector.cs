using UnityEditor;
using UnityEngine;
using vFrame.Core.Behaviours.Snapshots;

namespace vFrame.Core.Inspectors.Snapshots
{
    [CustomEditor(typeof(GameObjectSnapshotBehaviour))]
    public class GameObjectSnapshotBehaviourInspector : Editor
    {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            serializedObject.Update();
            
            if (GUILayout.Button("Take")) {
                var snapshot = target as GameObjectSnapshotBehaviour;
                if (null != snapshot) {
                    snapshot.Take();
                }
            }
            if (GUILayout.Button("Restore")) {
                var snapshot = target as GameObjectSnapshotBehaviour;
                if (null != snapshot) {
                    snapshot.Restore();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}