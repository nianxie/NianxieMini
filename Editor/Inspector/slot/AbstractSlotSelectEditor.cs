using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using UnityEditorInternal;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(AbstractSlotSelect), true)]
    public class AbstractSlotSelectEditor: UnityEditor.Editor
    {
        private AbstractSlotSelect slotSelect;
        private void OnEnable()
        {
            slotSelect = (AbstractSlotSelect) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!Application.isPlaying)
            {
                slotSelect.EditorInspectorUpdate();
            }
            if (slotSelect is SlotSelectBody selectBody)
            {
                if (selectBody.selectHead == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(SlotSelectHead)} is required in parent.", MessageType.Error);
                    if (!Application.isPlaying)
                    {
                        if (GUILayout.Button("fix select head"))
                        {
                            selectBody.transform.parent.gameObject.AddComponent<SlotSelectHead>();
                        }
                    }
                }
            } else if (slotSelect is SlotSelectHead selectHead)
            {
                if (selectHead.selectBody == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(SlotSelectBody)} is required in child.", MessageType.Error);
                    if (!Application.isPlaying)
                    {
                        if (GUILayout.Button("fix body"))
                        {
                            selectHead.EnsureSelectBodyInChild();
                        }
                    }
                }
            }
        }
    }
}
