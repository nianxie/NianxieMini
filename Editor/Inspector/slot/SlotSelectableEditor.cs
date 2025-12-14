using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using UnityEditorInternal;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(SlotSelectable), true)]
    public class SlotSelectableEditor: UnityEditor.Editor
    {
        private SlotSelectable selectable;
        private void OnEnable()
        {
            selectable = (SlotSelectable) target;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!Application.isPlaying)
            {
                selectable.ON_INSPECTOR_UPDATE();
            }
        }
    }
}
