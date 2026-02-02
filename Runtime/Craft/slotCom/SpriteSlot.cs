using System;
using System.Collections.Generic;
using System.Net;
using Nianxie.Riff;
using Nianxie.Utils;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SlotSelectHead))]
    public class SpriteSlot : AbstractRenderSlot<Sprite, SpriteJson>
    {
        [SerializeField] 
        private Sprite m_DefaultSprite;
        public Sprite defaultSprite => m_DefaultSprite;
        private AssignedSprite m_AssignedSprite;

        private Sprite currentSprite => m_AssignedSprite!=null?m_AssignedSprite.sprite:m_DefaultSprite;
        
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
            if (injected is SlotInjected.DefaultInjected defaultInjected && m_DefaultSprite != null)
            {
                var defaultPath = string.Join(',', defaultInjected.keys);
                injected.behav.slotHandler.RegisterBuiltinObject(defaultPath, m_DefaultSprite);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            spriteRenderer.sprite = currentSprite;
        }

        protected override SpriteJson TypedPackToJson(IPackContext packContext)
        {
            var json = new SpriteJson();
            if (m_AssignedSprite != null)
            {
                var usage = m_AssignedSprite.usage;
                if (usage.sourceKind is BuiltinSourceKind builtinSourceKind)
                {
                    json.builtinPath = builtinSourceKind.builtinPath;
                    json.riffIndex = -1;
                }
                else if(usage.sourceKind is PackableSourceKind packableSourceKind)
                {
                    json.riffIndex = packableSourceKind.packRiffIndex;
                }
                else
                {
                    throw new Exception($"unexpected source kind {usage.sourceKind.GetType()}");
                }
                json.meta = m_AssignedSprite.meta;
            }
            else
            {
                if (slotHandler.IsBuiltinObject(defaultSprite, out var builtinPath))
                {
                    json.builtinPath=builtinPath;
                    json.meta=new SpriteMeta()
                    {
                        rect=IntRectangle.FromUnityRect(defaultSprite.textureRect),
                        pivot=defaultSprite.pivot,
                        pixelsPerUnit=defaultSprite.pixelsPerUnit,
                    };
                }
                else
                {
                    throw new Exception("default sprite is not builtin");
                }
            }
            return json;
        }

        protected override void TypedUnpackFromJson(UnpackContext unpackContext, SpriteJson slotJson)
        {
            var usage = unpackContext.GetTextureUsage(slotJson.builtinPath, slotJson.riffIndex);
            m_AssignedSprite = usage.UseAndCreateSprite(slotJson.meta);
            spriteRenderer.sprite = m_AssignedSprite.sprite;
        }

        public void Assign(TextureUsage texUsage)
        {
            // TODO 根据fitx和fity对sprite进行裁切。
            // TODO 这里rect和pivot填写的不太好，也好像不正确，考虑等会儿优化一下。
            var width = defaultSprite.rect.width;
            var height = defaultSprite.rect.height;
            var newAssignedSprite = texUsage.UseAndCreateSprite(new SpriteMeta
            {
                rect=new IntRectangle(0, 0, Mathf.RoundToInt(width), Mathf.RoundToInt(height)), 
                pivot=Vector2.zero, 
                pixelsPerUnit=100,
            });
            if(m_AssignedSprite!=null)
            {
                m_AssignedSprite.usage.DelUsage(m_AssignedSprite);
            }
            m_AssignedSprite = newAssignedSprite;
            spriteRenderer.sprite = currentSprite;
            if (TryGetComponent<PolygonSlot>(out var polygonSlot))
            {
                polygonSlot.CalculatePolygon(newAssignedSprite.sprite);
            }
        }

        private void OnDestroy()
        {
            if(m_AssignedSprite!=null)
            {
                m_AssignedSprite.usage.DelUsage(m_AssignedSprite);
                m_AssignedSprite = null;
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