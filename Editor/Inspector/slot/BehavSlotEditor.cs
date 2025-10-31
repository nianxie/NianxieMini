using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Components;
using Nianxie.Craft;
using UnityEditorInternal;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(BehavSlot), false)]
    public class BehavSlotEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
