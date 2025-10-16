using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using UnityEditorInternal;
using XLua;

namespace Nianxie.Editor
{
    // The custom editor of the SgLuaMonoBehaviourEditor class.
    [CustomEditor(typeof(SlotBehaviour), false)]
    public class SlotBehaviourEditor : LuaBehaviourEditor
    {
        protected SlotBehaviour slotBehav => (SlotBehaviour) m_behav;
        private Action drawOneInjection(Rect rect, AbstractReflectInjection injection)
        {
            Action lateAction = null;
            var rect1 = new Rect(rect.x, rect.y, rect.width/3, rect.height);
            var rect23 = new Rect(rect.x+rect.width/3, rect.y, 2*rect.width/3, rect.height);
            EditorGUI.LabelField(rect1, injection.key);
            if (injection is AbstractNodeInjection nodeInjection && nodeInjection.collectionKind == InjectionMultipleKind.Single)
            {
                var rect2 = new Rect(rect.x+rect.width/3, rect.y, rect.width/3, rect.height);
                var rect3 = new Rect(rect.x+2*rect.width/3, rect.y, rect.width/3, rect.height);
                var obj = nodeInjection.ToNodeObject(m_behav, nodeInjection.nodePath);
                if(obj == null)
                {
                    EditorGUI.LabelField(rect23, $"node missing : {nodeInjection.nodePath}", m_errStyle);
                }
                else
                {
                    EditorGUI.ObjectField(rect2, obj, injection.csharpType, false);
                    if (obj is SpriteSlot spriteSlot)
                    {
                        if (GUI.Button(rect3, "Upload"))
                        {
                            lateAction = () =>
                            {
                                var pngPath = EditorUtility.OpenFilePanel("Upload Image", Path.Combine(Application.dataPath, ".."), "png");
                                var pngData = File.ReadAllBytes(pngPath);
                                var tex = new Texture2D(2,2);
                                tex.LoadImage(pngData);
                                if (slotBehav.editorTexDict.TryGetValue(injection.key, out var oldTex))
                                {
                                    if (oldTex != null)
                                    {
                                        UnityEngine.Object.DestroyImmediate(oldTex);
                                    }
                                }
                                slotBehav.editorTexDict[injection.key] = tex;
                                spriteSlot.WriteRawData(tex);
                            };
                        }
                    }
                    //EditorGUI.ObjectField(rect3, obj, injection.csharpType, false);
                }
            }
            else
            {
                EditorGUI.LabelField(rect23, $"slot script field invalid: {injection.key}", m_errStyle);
            }

            return lateAction;
        }
        protected override void DrawInjections(AbstractReflectEnv reflectEnv, WarmedReflectClass reflectInfo)
        {
            Action lateAction = null;
            var injectionList = new ReorderableList(reflectInfo.injections, typeof(AbstractReflectInjection), false, true, false, false);
            injectionList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Lua Injection List");
            injectionList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                lateAction = drawOneInjection(rect, reflectInfo.injections[index]) ?? lateAction;
            };
            injectionList.DoLayoutList();
            lateAction?.Invoke();
        }
    }
}
