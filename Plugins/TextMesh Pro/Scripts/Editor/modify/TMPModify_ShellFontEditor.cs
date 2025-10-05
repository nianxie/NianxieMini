using UnityEngine;
using UnityEditor;

namespace TMPro.EditorUtilities
{
    
    [CustomEditor(typeof(TMPModify_ShellFont))]
    public class TMPModify_ShellFontEditor: Editor
    {
        private TMPModify_ShellFont shellFont;
        SerializedProperty fontName;
        protected virtual void OnEnable()
        {
            shellFont = (TMPModify_ShellFont) target;
            fontName = serializedObject.FindProperty(nameof(fontName));
        }

        public override void OnInspectorGUI()
        {
            var beforeFontName = fontName.stringValue;
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            var afterFontName = fontName.stringValue;
            if (EditorGUI.EndChangeCheck())
            {
                if (beforeFontName != afterFontName)
                {
                    shellFont.Refresh();
                }
                else
                {
                    shellFont.RefreshMaterial();
                }
            }
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Font Asset", shellFont.fontAsset, typeof(TMP_FontAsset), false);
            EditorGUILayout.ObjectField("Material", shellFont.material, typeof(Material), false);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Reset Material"))
            {
                DestroyImmediate(shellFont.material);
                shellFont.RefreshMaterial();
            }
        }
    }

}
