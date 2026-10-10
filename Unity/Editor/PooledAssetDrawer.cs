using UnityEditor;
using UnityEngine;

namespace Yogurt.Unity
{
    [CustomPropertyDrawer(typeof(PooledAsset<>))]
    internal class PooledAssetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty asset = property.FindPropertyRelative(nameof(PooledAsset<Component>.Asset));
            AssetDrawer.DrawPrefab(position, asset, label);
        }
    }
}
