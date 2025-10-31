using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using UnityEditorInternal;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(PositionSlot), true)]
    public class PositionSlotEditor: UnityEditor.Editor
    {
        protected PositionSlot posSlot;
        protected void OnEnable()
        {
            posSlot = (PositionSlot) target;
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            DrawDefaultInspector();
            EditorGUI.EndDisabledGroup();
            if (!Application.isPlaying)
            {
                if (posSlot.GetComponentIndex() > 1)
                {
                    ComponentUtility.MoveComponentUp(posSlot);
                }
                posSlot.ON_INSPECTOR_UPDATE(false);
            }
        }
    }
}
