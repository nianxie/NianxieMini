using Nianxie.Craft;
using UnityEditor;
using UnityEngine;

namespace Nianxie.Editor
{
    [CustomPropertyDrawer(typeof(AbstractSlotValue), true)]
    public class SlotValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative(nameof(SlotValue<string>.defaultValue));
            EditorGUI.PropertyField(position, valueProperty, true);
        }
    }
}