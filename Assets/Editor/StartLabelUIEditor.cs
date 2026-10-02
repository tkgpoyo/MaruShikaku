using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace MaruSikaku.Editor
{
    [CustomEditor(typeof(StartLabelUI))]
    public class StartLabelUIEditor : TMP_EditorPanelUI
    {
        protected override void DrawExtraSettings()
        {
            base.DrawExtraSettings();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("アニメーション関連", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_pathTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_path"), includeChildren: true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_breakTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_radius"));
        }
    }
}