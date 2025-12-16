using System.Reflection;
using Nianxie.Craft;
using UnityEditor;
using UnityEngine;

namespace Nianxie.Editor
{
    [CustomPropertyDrawer(typeof(SlotValueAttribute))]
    public class SlotValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. 绘制折叠框（类似 Unity 内置 struct 风格）
            //EditorGUI.BeginProperty(position, label, property);
            EditorGUILayout.LabelField(label);
            // 缩进（模拟 Unity 内置样式）
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indent + 1;

            // 2. 获取 struct 内的字段（通过 SerializedProperty 访问）
            SerializedProperty defaultValue = property.FindPropertyRelative(nameof(SlotValue<int>.defaultValue));
            SerializedProperty assignedValue = property.FindPropertyRelative(nameof(SlotValue<int>.assignedValue));
            SerializedProperty isAssigned = property.FindPropertyRelative(nameof(SlotValue<int>.isAssigned));

            // 3. 绘制每个字段
            EditorGUILayout.PropertyField(defaultValue);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(assignedValue);
            EditorGUILayout.PropertyField(isAssigned);
            EditorGUI.EndDisabledGroup();
            
            // 恢复缩进
            EditorGUI.indentLevel = indent;
            //EditorGUI.EndProperty();
        }
        
    }
}