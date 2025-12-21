using System;
using Cysharp.Threading.Tasks.Triggers;
using Nianxie.Utils;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Nianxie.Craft
{

    [RequireComponent(typeof(SlotSelectHead))]
    [ExecuteAlways]
    public class TextSlot : AbstractAssetSlot
    {
        private const string TEXT_NODE_NAME = "::text";
        [SerializeField]
        private TextMeshPro m_TextMeshPro;

        [SlotValue]
        [SerializeField] 
        private SlotValue<string> m_SlotValue;
        
        public override object slotValue {
            get => m_SlotValue.ReadValue();
            set
            {
                var text = (string) value;
                m_SlotValue.AssignValue(text);
                m_TextMeshPro.text = text;
            }
        }
        private DrivenRectTransformTracker m_RectTracker = new DrivenRectTransformTracker();
        
        private void OnEnable()
        {
            RefreshTrack();
        }

        private void RefreshTrack()
        {
            m_RectTracker.Clear();
            if (m_TextMeshPro != null)
            {
                m_RectTracker.Add(selectHead, m_TextMeshPro.rectTransform, DrivenTransformProperties.All);
            }
        }

        private void OnDestroy()
        {
            m_RectTracker.Clear();
        }

        public override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            throw new NotImplementedException();
        }

        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            throw new NotImplementedException();
        }
        
#if UNITY_EDITOR
        [BlackList]
        public override void EditorInspectorUpdate(bool change)
        {
            if (m_TextMeshPro == null)
            {
                return;
            }
            // 更新textmeshpro的font、text、gameObject.name
            m_TextMeshPro.GetFont(out var shellFont, out var _);
            if (shellFont == null)
            {
                m_TextMeshPro.SetFont(NianxieEditorConst.LoadStandRes().shellFont, null);
                UnityEditor.EditorUtility.SetDirty(m_TextMeshPro);
            }

            if (m_TextMeshPro.gameObject.name != TEXT_NODE_NAME || m_TextMeshPro.text != m_SlotValue.defaultValue)
            {
                m_TextMeshPro.gameObject.name = TEXT_NODE_NAME;
                m_TextMeshPro.text = m_SlotValue.defaultValue;
                UnityEditor.EditorUtility.SetDirty(m_TextMeshPro.gameObject);
                UnityEditor.EditorUtility.SetDirty(m_TextMeshPro);
            }

            var selectBody = selectHead.selectBody;
            if (selectBody == null)
            {
                return;
            }

            var spriteRenderer = selectBody.spriteRenderer;
            var modifyBackground = false;
            if (spriteRenderer.drawMode != SpriteDrawMode.Sliced)
            {
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                modifyBackground = true;
            }
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = NianxieEditorConst.LoadStandRes().sliced9;
                modifyBackground = true;
            }
            
            if (m_TextMeshPro != null)
            {
                var rectTrans = m_TextMeshPro.rectTransform;
                var half = 0.5f * Vector2.one;
                if (rectTrans.pivot != half || rectTrans.anchorMax != half || rectTrans.anchorMin != half)
                {
                    rectTrans.pivot = 0.5f * Vector2.one;
                    rectTrans.anchorMin = 0.5f * Vector2.one;
                    rectTrans.anchorMax = 0.5f * Vector2.one;
                    UnityEditor.EditorUtility.SetDirty(rectTrans);
                }

                if (rectTrans.localScale != Vector3.one)
                {
                    rectTrans.localScale = Vector3.one;
                    UnityEditor.EditorUtility.SetDirty(rectTrans);
                }

                if (rectTrans.localRotation != Quaternion.identity)
                {
                    rectTrans.localRotation = Quaternion.identity;
                    UnityEditor.EditorUtility.SetDirty(rectTrans);
                }
                
                var pos = 0.1f * Vector3.back;
                if (rectTrans.localPosition != pos)
                {
                    rectTrans.localPosition = pos;
                    UnityEditor.EditorUtility.SetDirty(rectTrans);
                }

                if (rectTrans.sizeDelta != spriteRenderer.size)
                {
                    rectTrans.sizeDelta = spriteRenderer.size;
                    UnityEditor.EditorUtility.SetDirty(rectTrans);
                }
            }

            if (modifyBackground)
            {
                UnityEditor.EditorUtility.SetDirty(spriteRenderer);
            }
        }
        private void Reset()
        {
            if (selectHead.selectBody == null)
            {
                selectHead.Reset();
            }

            var selectBody = selectHead.selectBody;
            if (selectBody == null)
            {
                Debug.LogError("SlotSelectBody create failed");
                return;
            }

            TextMeshPro textCom = null;
            for (int i = 0; i < selectBody.transform.childCount; i++)
            {
                var child = selectBody.transform.GetChild(i);
                if (child.TryGetComponent(out TextMeshPro tmp) && child.gameObject.name == TEXT_NODE_NAME)
                {
                    textCom = tmp;
                    break;
                }
            }

            if (textCom == null)
            {
                var bodyGo = new GameObject(TEXT_NODE_NAME, typeof(TextMeshPro));
                bodyGo.transform.SetParent(selectBody.transform, false);
                UnityEditor.Undo.RegisterCreatedObjectUndo(bodyGo, "Create Child Object");
                m_TextMeshPro = bodyGo.GetComponent<TextMeshPro>();
                m_TextMeshPro.autoSizeTextContainer = true;
                m_TextMeshPro.fontSize = 2;
                m_TextMeshPro.fontSizeMin = 0.1f;
                m_TextMeshPro.SetFont(NianxieEditorConst.LoadStandRes().shellFont, null);
                bodyGo.transform.localPosition = Vector3.zero;
                bodyGo.transform.localRotation = Quaternion.identity;
            }
            else
            {
                m_TextMeshPro = textCom;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            RefreshTrack();
        }

#endif
    }
}