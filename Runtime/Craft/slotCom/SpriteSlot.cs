using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSlot : AbstractAssetSlot
    {
        [NonSerialized] SpriteRenderer m_Renderer;
        private SpriteRenderer spriteRenderer
        {
            get
            {
                if (!m_Renderer)
                {
                    gameObject.TryGetComponent(out m_Renderer);
                }
                return m_Renderer;
            }
        }
        
        [SerializeField]
        private bool m_FitX;
        
        [SerializeField]
        private bool m_FitY;

        [SlotValue]
        [SerializeField]
        private SlotValue<Sprite> m_SlotValue;
        
        public override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            throw new System.NotImplementedException();
        }

        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            throw new System.NotImplementedException();
        }

        public override object slotValue {
            get => m_SlotValue.ReadValue();
            set
            {
                // TODO 根据fitx和fity对sprite进行裁切。
                var sprite = (Sprite) value;
                slotCallback.Incref(this, sprite.texture);
                if (m_SlotValue.assignedValue != null)
                {
                    slotCallback.Decref(this, m_SlotValue.assignedValue.texture);
                }
                m_SlotValue.AssignValue(sprite);
                spriteRenderer.sprite = sprite;
            }
        }

        public override void PostDuplicate()
        {
            if (m_SlotValue.assignedValue != null)
            {
                slotCallback.Incref(this, m_SlotValue.assignedValue.texture);
            }
        }
#if UNITY_EDITOR
        [BlackList]
        public override void ON_INSPECTOR_UPDATE(bool change)
        {

            var defaultSprite = m_SlotValue.defaultValue;
            if (defaultSprite != null)
            {
                var size = defaultSprite.rect.size / defaultSprite.pixelsPerUnit;
                var pivot = defaultSprite.pivot / defaultSprite.rect.size;
                var rectTransform = selectable.rectTransform;
                if (size != rectTransform.rect.size || pivot != rectTransform.pivot)
                {
                    rectTransform.sizeDelta = size;
                    rectTransform.pivot = pivot;
                }

                if (spriteRenderer.sprite != defaultSprite)
                {
                    spriteRenderer.sprite = defaultSprite;
                    UnityEditor.EditorUtility.SetDirty(spriteRenderer);
                }
            }
            else
            {
                // TODO 应该考虑提供一张默认的Sprite，在未设置DefaultSprite的时候自动将DefaultSprite设置为默认的Sprite。
                spriteRenderer.sprite = null;
            }

            base.ON_INSPECTOR_UPDATE(change);
        }
#endif
    }
}
