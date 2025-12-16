using System;
using Nianxie.Utils;
using TMPro;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{

    [RequireComponent(typeof(SlotSelectable))]
    public class TextSlot : AbstractAssetSlot
    {
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

        public override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            throw new NotImplementedException();
        }

        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            throw new NotImplementedException();
        }
        
        private void SyncBackgroundSize()
        {
            /*if (background)
            {
                if (background.drawMode == SpriteDrawMode.Sliced)
                {
                    var rectTransform = GetComponent<RectTransform>();
                    background.size = rectTransform.sizeDelta;
                }
            }*/
        }
        
        void OnRectTransformDimensionsChange()
        {
            SyncBackgroundSize();
        }
#if UNITY_EDITOR
        [BlackList]
        public string MakeLinkName()
        {
            return $"@{gameObject.name}";
        }

        [BlackList]
        public override void ON_INSPECTOR_UPDATE(bool change)
        {
            if (m_TextMeshPro == null)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var childTrans = transform.GetChild(i);
                    if (childTrans.gameObject.name.StartsWith("@") && childTrans.TryGetComponent<TextMeshPro>(out var textMeshPro))
                    {
                        m_TextMeshPro = textMeshPro;
                        UnityEditor.EditorUtility.SetDirty(this);
                        break;
                    }
                }

                if (m_TextMeshPro == null)
                {
                    var newGo = new GameObject(MakeLinkName(), typeof(TextMeshPro), typeof(RectTransform));
                    newGo.GetComponent<RectTransform>().SetParent(transform);
                    m_TextMeshPro = newGo.GetComponent<TextMeshPro>();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
            var linkName = MakeLinkName();
            // 更新textmeshpro的font、text、gameObject.name
            m_TextMeshPro.GetFont(out var shellFont, out var _);
            if (shellFont == null || m_TextMeshPro.gameObject.name != linkName || m_TextMeshPro.text != m_SlotValue.defaultValue)
            {
                m_TextMeshPro.gameObject.name = linkName;
                m_TextMeshPro.text = m_SlotValue.defaultValue;
                m_TextMeshPro.SetFont(NianxieEditorConst.LoadStandRes().shellFont, null);
                UnityEditor.EditorUtility.SetDirty(m_TextMeshPro.gameObject);
                UnityEditor.EditorUtility.SetDirty(m_TextMeshPro);
            }
            // 更新textmeshpro的index
            if (m_TextMeshPro.transform.GetSiblingIndex() != 0)
            {
                m_TextMeshPro.transform.SetSiblingIndex(0);
            }
            // 更新textmeshpro的rectTransform
            var textTrans = m_TextMeshPro.rectTransform;
            if (textTrans.anchorMin != Vector2.zero || textTrans.anchorMax != Vector2.one || textTrans.sizeDelta != Vector2.zero || textTrans.anchoredPosition != Vector2.zero)
            {
                textTrans.anchorMin = Vector2.zero;
                textTrans.anchorMax = Vector2.one;
                textTrans.sizeDelta = Vector2.zero;
                textTrans.anchoredPosition = Vector2.zero;
                UnityEditor.EditorUtility.SetDirty(textTrans);
            }

            base.ON_INSPECTOR_UPDATE(change);
            var spriteRenderer = selectable.spriteRenderer;
            if (spriteRenderer.drawMode != SpriteDrawMode.Sliced)
            {
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                UnityEditor.EditorUtility.SetDirty(spriteRenderer);
            }

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = NianxieEditorConst.LoadStandRes().sliced9;
                UnityEditor.EditorUtility.SetDirty(spriteRenderer);
            }
        }
#endif
    }
}