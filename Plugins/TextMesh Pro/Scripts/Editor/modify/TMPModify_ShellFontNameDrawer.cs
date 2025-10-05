using System.Configuration;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace TMPro.EditorUtilities
{
    // 自定义属性抽屉，需放在Editor文件夹下
    [CustomPropertyDrawer(typeof(TMPModify_ShellFontNameAttribute))]
    public class TMPModify_ShellFontNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "仅支持string类型");
                return;
            }

            var nameArr = TMP_Settings.rawShellFonts.Select((nameFont) => nameFont.name).ToArray();
            var selectedIndex = nameArr.Select((name, index) => (name, index)).FirstOrDefault(e => e.name == property.stringValue).index;
            
            if (selectedIndex < 0) selectedIndex = 0; // 默认为第一个选项

            selectedIndex = EditorGUI.Popup(position, label, selectedIndex, nameArr);

            if (selectedIndex >= 0 && selectedIndex < nameArr.Length)
            {
                property.stringValue = nameArr[selectedIndex];
            }
        }
    }
}