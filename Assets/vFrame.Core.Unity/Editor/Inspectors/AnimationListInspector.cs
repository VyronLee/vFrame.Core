using System;
using UnityEditor;
using UnityEngine;
using vFrame.Core.Behaviours;

namespace vFrame.Core.Inspectors
{
    [CustomEditor(typeof(AnimationList))]
    public class AnimationListInspector : Editor
    {
        private SerializedProperty _animations;
        private bool _listVisibility = true;

        private void OnEnable() {
            _animations = serializedObject.FindProperty("_animations");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            var countInput = EditorGUILayout.TextField("Animation Count", _animations.arraySize.ToString());
            _animations.arraySize = int.Parse(countInput);

            _listVisibility = EditorGUILayout.Foldout(_listVisibility, "Animations");
            if (_listVisibility) {
                EditorGUI.indentLevel++;
                for (var i = 0; i < _animations.arraySize; i++) {
                    var elementProperty = _animations.GetArrayElementAtIndex(i);

                    var name = elementProperty.FindPropertyRelative("name");
                    var clip = elementProperty.FindPropertyRelative("clip");

                    var prevLabelWidth = EditorGUIUtility.labelWidth;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 60f;
                    EditorGUILayout.PropertyField(name);
                    EditorGUIUtility.labelWidth = 40f;
                    EditorGUILayout.PropertyField(clip);
                    EditorGUIUtility.labelWidth = prevLabelWidth;

                    var prevEnabled = GUI.enabled;
                    GUI.enabled = Application.isPlaying;
                    if (GUILayout.Button("Play", GUILayout.Width(50f))) {
                        var animationList = target as AnimationList;
                        if (animationList) {
                            animationList.Play(name.stringValue);
                        }
                    }
                    GUI.enabled = prevEnabled;

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}