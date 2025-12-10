using System.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;
using System.Collections.Generic;
using UnityEngine;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public abstract class AbstractNodeSlot : AbstractSlotCom, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
    {
        [NonSerialized] BoxCollider2D m_collider2D;
        public BoxCollider2D touchCollider2D
        {
            get
            {
                if (!m_collider2D)
                {
                    gameObject.TryGetComponent(out m_collider2D);
                }
                return m_collider2D;
            }
        }

        public void OperRemoveSelf()
        {
            if (transform.parent.TryGetComponent<ListSlot>(out var listSlot))
            {
                listSlot.OperRemoveElement(this);
            }
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            //craftModule.DispatchSlotPointer(this, nameof(IPointerDownHandler.OnPointerDown), eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!TryGetComponent<PositionSlot>(out var posSlot) || !posSlot.dragging)
            {
                craftEdit.OnSelect(this);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            //craftModule.DispatchSlotPointer(this, nameof(IPointerUpHandler.OnPointerUp), eventData);
        }
    }
}
