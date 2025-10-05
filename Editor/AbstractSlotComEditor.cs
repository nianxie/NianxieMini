using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using XLua;

namespace Nianxie.Editor
{
    // The custom editor of the SgLuaMonoBehaviourEditor class.
    [CustomEditor(typeof(AbstractSlotCom), true)]
    public class AbstractSlotComEditor: UnityEditor.Editor
    {
        protected AbstractSlotCom slotCom;
        protected void OnEnable()
        {
            slotCom = (AbstractSlotCom) target;
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                slotCom.OnInspectorChange();
            }
        }
    }
}
