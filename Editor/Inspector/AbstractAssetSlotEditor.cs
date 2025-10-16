using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using UnityEditorInternal;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(AbstractAssetSlot), true)]
    public class AbstractAssetSlotEditor: UnityEditor.Editor
    {
        protected AbstractAssetSlot slotCom;
        protected void OnEnable()
        {
            slotCom = (AbstractAssetSlot) target;
        }
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
                DrawDefaultInspector();
                EditorGUI.EndDisabledGroup();
                return;
            }

            var comIndex = slotCom.TryGetComponent<PositionSlot>(out _) ? 2 : 1;
            if(slotCom.GetComponentIndex() > comIndex)
            {
                ComponentUtility.MoveComponentUp(slotCom);
            }

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            var change = EditorGUI.EndChangeCheck();
            slotCom.OnInspectorUpdate(change);
        }
    }
}
