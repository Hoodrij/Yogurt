using UnityEditor;
using UnityEngine;

namespace Yogurt.Unity
{
    [CustomPropertyDrawer(typeof(Asset<>))]
    internal class AssetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawPrefab(position, property, label);
        }

        public static void DrawPrefab(Rect position, SerializedProperty asset, GUIContent label)
        {
            SerializedProperty prefab = asset.FindPropertyRelative(nameof(Asset<Component>.Prefab));

            EditorGUI.BeginProperty(position, label, asset);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.PropertyField(position, prefab, GUIContent.none);
            EditorGUI.EndProperty();
        }
    }
}
