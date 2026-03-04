using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Nianxie.Editor
{
    public static class UnityEditorInternalBridge
    {
        public static void ObjectListArea_postAssetIconDrawCallback_add(System.Action<Rect, string, bool> onAssetIconDraw)
        {
            ObjectListArea.postAssetIconDrawCallback += new ObjectListArea.OnAssetIconDrawDelegate(onAssetIconDraw);
        }
        public static void AssetsTreeViewGUI_postAssetIconDrawCallback_add(System.Action<Rect, string> onAssetIconDraw)
        {
            AssetsTreeViewGUI.postAssetIconDrawCallback += new AssetsTreeViewGUI.OnAssetIconDrawDelegate(onAssetIconDraw);
        }
        public static void EditorGUIUtility_beginProperty_add(System.Action<Rect, SerializedProperty> onBeginProperty)
        {
            EditorGUIUtility.beginProperty += onBeginProperty;
        }
        public static bool DrivenPropertyManagerInternal_IsDriven(Object target, string propertyPath)
        {
            return DrivenPropertyManagerInternal.IsDriven(target, propertyPath);
        }
        public static bool DrivenPropertyManagerInternal_IsDriving(Object driver, Object target, string propertyPath)
        {
            return DrivenPropertyManagerInternal.IsDriving(driver, target, propertyPath);
        }
    }
}
