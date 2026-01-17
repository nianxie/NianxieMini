using System;
using System.Collections.Generic;
using Nianxie.Utils;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SlotSelectHead))]
    public class SpriteSlot : AbstractRenderSlot<Sprite, SpriteJson>
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

        public override void Init(SlotInjected injected)
        {
            base.Init(injected);
            if (injected is SlotInjected.DefaultInjected defaultInjected)
            {
                var defaultPath = string.Join(',', defaultInjected.keys);
                injected.behav.slotHandler.RegisterDefaultObject(defaultPath, m_SlotValue.defaultValue);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            spriteRenderer.sprite = m_SlotValue.Get();
        }

        protected override SpriteJson TypedPackToJson(IPackContext packContext)
        {
            var sprite = m_SlotValue.Get();
            if (slotHandler.IsDefaultObject(sprite, out var defaultPath))
            {
                return new SpriteJson()
                {
                    defaultPath=defaultPath,
                    sprite=-1,
                };
            }
            else
            {
                var index = packContext.PutSprite(m_SlotValue.Get());
                return new SpriteJson()
                {
                    defaultPath=null,
                    sprite=index,
                };
            }
        }

        protected override void TypedUnpackFromJson(UnpackContext unpackContext, SpriteJson slotJson)
        {
            var spriteJson = slotJson;
            var sprite = unpackContext.GetSprite(spriteJson.sprite);
            m_SlotValue.defaultValue = sprite;
            spriteRenderer.sprite = sprite;
        }

        public override void AssignValue(Sprite inputSprite)
        {
            // TODO 根据fitx和fity对sprite进行裁切。
            var sprite = Sprite.Create(inputSprite.texture, inputSprite.rect, Vector2.one*0.5f);
            slotHandler.Incref(this, sprite.texture);
            var oldValue = m_SlotValue.Set(sprite);
            if (oldValue != null)
            {
                slotHandler.Decref(this, oldValue.texture);
                Destroy(oldValue);
            }
            spriteRenderer.sprite = sprite;
        }

        private void OnDestroy()
        {
            var oldValue = m_SlotValue.Set(null);
            if (oldValue != null)
            {
                slotHandler.Decref(this, oldValue.texture);
                Destroy(oldValue);
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