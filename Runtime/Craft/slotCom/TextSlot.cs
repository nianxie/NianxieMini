using System;
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

        [SerializeField]
        private string m_DefaultText;
        
        [HideInInspector]
        [SerializeField]
        private string m_UserText;

        public string ReadText()
        {
            return string.IsNullOrEmpty(m_UserText) ? m_DefaultText : m_UserText;
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
            base.ON_INSPECTOR_UPDATE(change);
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
            if (m_TextMeshPro.gameObject.name != linkName || m_TextMeshPro.text != ReadText())
            {
                m_TextMeshPro.gameObject.name = linkName;
                m_TextMeshPro.text = ReadText();
                UnityEditor.EditorUtility.SetDirty(m_TextMeshPro.gameObject);
            }
            if (m_TextMeshPro.transform.GetSiblingIndex() != 0)
            {
                m_TextMeshPro.transform.SetSiblingIndex(0);
            }
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
        }
#endif
    }
}