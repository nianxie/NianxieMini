using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(RectTransform))]
    public class SlotSelectable: MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
    {
        [NonSerialized] RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (!m_RectTransform)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }
                return m_RectTransform;
            }
        }
        
        [NonSerialized] BoxCollider2D m_Collider2D;
        public BoxCollider2D touchCollider2D
        {
            get
            {
                if (!m_Collider2D)
                {
                    m_Collider2D = GetComponent<BoxCollider2D>();
                }
                return m_Collider2D;
            }
        }
        
        [NonSerialized] SpriteRenderer m_SpriteRenderer;
        public SpriteRenderer spriteRenderer
        {
            get
            {
                if (!m_SpriteRenderer)
                {
                    m_SpriteRenderer = GetComponent<SpriteRenderer>();
                }
                return m_SpriteRenderer;
            }
        }
        private AbstractRenderSlot renderSlot;
        private SlotBehaviour slotBehav;
        private IUnionSlot unionSlot => slotBehav != null ? slotBehav : renderSlot;
        private void Awake()
        {
            slotBehav = GetComponent<SlotBehaviour>();
            renderSlot = GetComponent<AbstractRenderSlot>();
        }
        
        public bool IsList()
        {
            var slotField = unionSlot.slotField;
            return slotField != null && slotField.injection.multipleKind == InjectionMultipleKind.List;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            unionSlot.slotCallback.OnSelect(this);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
        }
        public void DuplicateSelf()
        {
            if (IsList())
            {
                var slotField = unionSlot.slotField;
                slotField.behav.GetSlotList(slotField.injection).DuplicateElement(this);
            }
        }
        public void DeleteSelf()
        {
            if (IsList())
            {
                var slotField = unionSlot.slotField;
                slotField.behav.GetSlotList(slotField.injection).DeleteElement(this);
            }
        }
#if UNITY_EDITOR
        [BlackList]
        public void ON_INSPECTOR_UPDATE()
        {
            var selfRect = rectTransform.rect;
            var selfCollider2D = touchCollider2D;
            if (selfCollider2D.size != selfRect.size || selfCollider2D.offset != selfRect.center)
            {
                selfCollider2D.size = new Vector2(selfRect.width, selfRect.height);
                selfCollider2D.offset = selfRect.center;
                UnityEditor.EditorUtility.SetDirty(selfCollider2D);
            }

            if (spriteRenderer.drawMode == SpriteDrawMode.Sliced && spriteRenderer.size != selfRect.size)
            {
                spriteRenderer.size = selfRect.size;
                UnityEditor.EditorUtility.SetDirty(spriteRenderer);
            }
        }
#endif
    }
}