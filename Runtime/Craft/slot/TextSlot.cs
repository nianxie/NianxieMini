using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(TextMeshPro))]
    public class TextSlot : AbstractSlotCom<TextJson, string, string>
    {
        public SpriteRenderer background;
        [NonSerialized] TextMeshPro m_TextMeshPro;
        public TextMeshPro drawText
        {
            get
            {
                if (!m_TextMeshPro)
                {
                    gameObject.TryGetComponent(out m_TextMeshPro);
                }
                return m_TextMeshPro;
            }
        }

        protected override TextJson PackFromSource(AbstractPackContext packContext, string source)
        {
            return new TextJson()
            {
                text=source,
            };
        }

        protected override string UnpackToTarget(CraftUnpackContext unpackContext, TextJson slotJson)
        {
            return slotJson.text;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SyncBackgroundSize();
        }

        private void SyncBackgroundSize()
        {
            if (background)
            {
                if (background.drawMode == SpriteDrawMode.Sliced)
                {
                    var rectTransform = GetComponent<RectTransform>();
                    background.size = rectTransform.sizeDelta;
                }
            }
        }
        
        void OnRectTransformDimensionsChange()
        {
            SyncBackgroundSize();
        }

        protected override void OnChangeValue()
        {
            drawText.text = target;
        }

        protected override string ValueProcess(string source)
        {
            return source;
        }

        protected override void DestroyTarget(string unpackOutput)
        {
            // do nothing
        }
    }
}