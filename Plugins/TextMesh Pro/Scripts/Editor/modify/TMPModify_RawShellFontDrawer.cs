using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
    [CustomPropertyDrawer(typeof(TMPModify_RawShellFont))]
    public class TMPModify_RawShellFontDrawer:PropertyDrawer
    {
        private static readonly GUIContent noLabel = new GUIContent("");
        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty nameProp = property.FindPropertyRelative(nameof(TMPModify_RawShellFont.name));
            SerializedProperty fontProp = property.FindPropertyRelative(nameof(TMPModify_RawShellFont.fontAsset));

            // 计算Rect位置
            Rect rect1 = new Rect(position.x, position.y, position.width/2, EditorGUIUtility.singleLineHeight);
            Rect rect2 = new Rect(position.x+position.width/2, position.y, position.width/2, EditorGUIUtility.singleLineHeight);

            // 绘制name & font
            EditorGUI.LabelField(rect1, nameProp.stringValue);
            EditorGUI.PropertyField(rect2, fontProp, noLabel);
            var fontAsset = fontProp.objectReferenceValue as TMP_FontAsset;
            if (fontAsset != null)
            {
                nameProp.stringValue = System.IO.Path.GetFileNameWithoutExtension(fontAsset.name).Split(" ")[0];
            }
            else
            {
                nameProp.stringValue = "";
            }

            // 恢复缩进设置
            EditorGUI.indentLevel = indentLevel;
        }
    }
}