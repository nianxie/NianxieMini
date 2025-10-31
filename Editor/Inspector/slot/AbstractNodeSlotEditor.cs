using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using UnityEditorInternal;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(AbstractNodeSlot), true)]
    public class AbstractNodeSlotEditor: UnityEditor.Editor
    {
        protected AbstractNodeSlot slotCom;
        protected void OnEnable()
        {
            slotCom = (AbstractNodeSlot) target;
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
            slotCom.ON_INSPECTOR_UPDATE(change);
        }
    }
}
