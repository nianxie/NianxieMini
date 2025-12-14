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

        [SerializeField]
        private Sprite m_DefaultSprite;
        
        [HideInInspector]
        [SerializeField]
        private Sprite m_UserSprite;
        public override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            throw new System.NotImplementedException();
        }

        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            throw new System.NotImplementedException();
        }

        public Sprite ReadSprite()
        {
            return m_UserSprite==null?m_DefaultSprite:m_UserSprite;
        }

        public void WriteSprite(Sprite writeSprite)
        {
            // TODO 根据fitx和fity对sprite进行裁切。
            slotCallback.Incref(this, writeSprite.texture);
            if (m_UserSprite != null)
            {
                slotCallback.Decref(this, m_UserSprite.texture);
            }
            m_UserSprite = writeSprite;
            spriteRenderer.sprite = m_UserSprite;
        }
        
        public override void PostDuplicate()
        {
            if (m_UserSprite != null)
            {
                slotCallback.Incref(this, m_UserSprite.texture);
            }
        }
#if UNITY_EDITOR
        [BlackList]
        public override void ON_INSPECTOR_UPDATE(bool change)
        {
            if (m_UserSprite != null)
            {
                m_UserSprite = null;
                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (m_DefaultSprite != null)
            {
                var size = m_DefaultSprite.rect.size / m_DefaultSprite.pixelsPerUnit;
                var pivot = m_DefaultSprite.pivot / m_DefaultSprite.rect.size;
                if (size != rectTransform.rect.size || pivot != rectTransform.pivot)
                {
                    rectTransform.sizeDelta = size;
                    rectTransform.pivot = pivot;
                }

                if (spriteRenderer.sprite != m_DefaultSprite)
                {
                    spriteRenderer.sprite = m_DefaultSprite;
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
