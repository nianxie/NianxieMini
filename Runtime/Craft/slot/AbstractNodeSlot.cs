using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public abstract class AbstractNodeSlot : AbstractSlotCom, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
    {
        [NonSerialized] RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (!m_RectTransform)
                {
                    gameObject.TryGetComponent(out m_RectTransform);
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
                    gameObject.TryGetComponent(out m_Collider2D);
                }
                return m_Collider2D;
            }
        }

        public void DuplicateSelf()
        {
            if (transform.parent.TryGetComponent<TableSlot>(out var listSlot))
            {
                listSlot.DuplicateChild(this);
            }
        }

        public virtual void PostDuplicate()
        {
        }

        public void DeleteSelf()
        {
            if (transform.parent.TryGetComponent<TableSlot>(out var listSlot))
            {
                listSlot.DeleteChild(this);
            }
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            //craftModule.DispatchSlotPointer(this, nameof(IPointerDownHandler.OnPointerDown), eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            slotCallback.OnSelect(this);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            //craftModule.DispatchSlotPointer(this, nameof(IPointerUpHandler.OnPointerUp), eventData);
        }
#if UNITY_EDITOR
        [BlackList]
        public override void ON_INSPECTOR_UPDATE(bool change)
        {
            var selfRect = rectTransform.rect;
            var selfCollider2D = touchCollider2D;
            if (selfCollider2D.size != selfRect.size || selfCollider2D.offset != selfRect.center)
            {
                selfCollider2D.size = new Vector2(selfRect.width, selfRect.height);
                selfCollider2D.offset = selfRect.center;
                UnityEditor.EditorUtility.SetDirty(selfCollider2D);
            }
        }
#endif
    }
}
