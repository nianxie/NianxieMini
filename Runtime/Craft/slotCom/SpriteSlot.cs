using System;
using Nianxie.Utils;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

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
        
        [NonEditable]
        [SerializeField]
        private Vector2 m_Pivot;

        public Vector2 pivot => m_Pivot;
        
        [NonEditable]
        [SerializeField]
        private Vector2 m_Size;

        public Vector2 size => m_Size;
        
        [SerializeField]
        private bool m_FitX;
        
        [SerializeField]
        private bool m_FitY;

        [SerializeField]
        private SlotValue<Sprite> m_SlotValue;
        
        public override AbstractSlotJson PackToJson(IPutAsset putAsset)
        {
            var index = putAsset.PutSprite(m_SlotValue.ReadValue());
            var json = new SpriteJson()
            {
                sprite=index,
            };
            return json;
        }

        public override void UnpackFromJson(IGetAsset getAsset, AbstractSlotJson slotJson)
        {
            var spriteJson = (SpriteJson) slotJson;
            var sprite = getAsset.GetSprite(spriteJson.sprite);
            m_SlotValue.defaultValue = sprite;
            spriteRenderer.sprite = sprite;
        }

        public override object GetValue()
        {
            return m_SlotValue.ReadValue();
        }

        public override void SetValue(object o)
        {
            // TODO 根据fitx和fity对sprite进行裁切。
            var inputSprite = m_SlotValue.SafeCast(o);
            var sprite = Sprite.Create(inputSprite.texture, inputSprite.rect, Vector2.one*0.5f);
            slotCallback.Incref(this, sprite.texture);
            if (m_SlotValue.assignedValue != null)
            {
                slotCallback.Decref(this, m_SlotValue.assignedValue.texture);
                Destroy(m_SlotValue.assignedValue);
            }
            m_SlotValue.AssignValue(sprite);
            spriteRenderer.sprite = sprite;
        }

        public override void PostDuplicate()
        {
            var originSprite = m_SlotValue.assignedValue;
            if (originSprite != null)
            {
                var copySprite = Sprite.Create(originSprite.texture, originSprite.rect, originSprite.pivot);
                slotCallback.Incref(this, originSprite.texture);
                m_SlotValue.assignedValue = copySprite;
                spriteRenderer.sprite = copySprite;
            }
            else
            {
                spriteRenderer.sprite = m_SlotValue.defaultValue;
            }
        }

        private void OnDestroy()
        {
            if (m_SlotValue.assignedValue != null)
            {
                slotCallback.Decref(this, m_SlotValue.assignedValue.texture);
                Destroy(m_SlotValue.assignedValue);
            }
        }
#if UNITY_EDITOR
        [BlackList]
        public override void EditorInspectorUpdate(NianxieDefaultAssets defaultAssets)
        {
            EditorLocalUpdate(defaultAssets);
        }
        [BlackList]
        public override void EditorLocalUpdate(NianxieDefaultAssets defaultAssets)
        {
            var defaultSprite = m_SlotValue.defaultValue;
            if (defaultSprite != null)
            {
                var selectBody = selectHead.selectBody;
                if (selectBody != null)
                {
                    var render = selectBody.spriteRenderer;
                    if (render.sprite != defaultSprite || render.drawMode != SpriteDrawMode.Simple)
                    {
                        render.sprite = defaultSprite;
                        render.drawMode = SpriteDrawMode.Simple;
                        UnityEditor.EditorUtility.SetDirty(render);
                    }
                }

                if (defaultSprite.pivot != m_Pivot || defaultSprite.rect.size != m_Size)
                {
                    m_Pivot = defaultSprite.pivot;
                    m_Size = defaultSprite.rect.size;
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
            else
            {
                // TODO 应该考虑提供一张默认的Sprite，在未设置DefaultSprite的时候自动将DefaultSprite设置为默认的Sprite。
                var selectBody = selectHead.selectBody;
                if (selectBody != null)
                {
                    selectBody.spriteRenderer.sprite = null;
                }
            }

        }
#endif
    }
}