using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(TextMeshPro))]
    public class TextSlot : AbstractAssetSlot<TextJson, string, string>
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

        protected override TextJson PackFromRawData(AbstractPackContext packContext, string source)
        {
            return new TextJson()
            {
                text=source,
            };
        }

        protected override string UnpackToFinalData(CraftUnpackContext unpackContext, TextJson slotJson)
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

        protected override void OnDataModify()
        {
            drawText.text = finalData;
        }

        protected override string DataProcess(string source)
        {
            return source;
        }

        protected override void DestroyFinalData(string unpackOutput)
        {
            // do nothing
        }
    }
}