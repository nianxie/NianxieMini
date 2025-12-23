using Nianxie.Craft;
using UnityEditor;
using UnityEngine;

namespace Nianxie.Editor
{
    [CustomPropertyDrawer(typeof(NonEditableAttribute))]
    public class NonEditableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(position, property, true);
            }
        }
    }
}