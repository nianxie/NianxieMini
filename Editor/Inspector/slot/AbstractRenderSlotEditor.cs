using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using Nianxie.Preview;
using UnityEditorInternal;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(AbstractRenderSlot), true)]
    public class AbstractRenderSlotEditor : UnityEditor.Editor
    {
        protected AbstractRenderSlot slotCom;

        protected void OnEnable()
        {
            slotCom = (AbstractRenderSlot) target;
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
            if (slotCom.GetComponentIndex() > comIndex)
            {
                ComponentUtility.MoveComponentUp(slotCom);
            }

            using (new LocalizationGroup(target))
            {
                EditorGUI.BeginChangeCheck();
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty iterator = serializedObject.GetIterator();
                for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
                {
                    using (new EditorGUI.DisabledScope(enterChildren))
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
            var change = EditorGUI.EndChangeCheck();
            slotCom.EditorInspectorUpdate(PreviewAssets.instance);
        }
    }
}