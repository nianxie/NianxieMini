using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SlotSelectHead))]
    public class SpriteSlot : AbstractAssetSlot
    {
        private SpriteRenderer spriteRenderer => selectHead.selectBody.spriteRenderer;
        
        [NonSerialized] SlotSelectHead m_SlotSelectHead;
        private SlotSelectHead selectHead
        {
            get
            {
                if (!m_SlotSelectHead)
                {
                    m_SlotSelectHead = GetComponent<SlotSelectHead>();
                }
                return m_SlotSelectHead;
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
            var selectBody = selectHead.selectBody;
            if (selectBody != null)
            {
                var render = selectBody.spriteRenderer;
                var defaultSprite = m_SlotValue.defaultValue;
                if (defaultSprite != null)
                {
                    if (render.sprite != defaultSprite || render.drawMode != SpriteDrawMode.Simple)
                    {
                        render.sprite = defaultSprite;
                        render.drawMode = SpriteDrawMode.Simple;
                        UnityEditor.EditorUtility.SetDirty(render);
                    }
                }
                else
                {
                    // TODO 应该考虑提供一张默认的Sprite，在未设置DefaultSprite的时候自动将DefaultSprite设置为默认的Sprite。
                    render.sprite = null;
                }
            }
        }
#endif
    }
}
