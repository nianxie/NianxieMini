using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SlotSelectable))]
    [DisallowMultipleComponent]
    public abstract class AbstractNodeSlot : AbstractSlotCom
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
        
        public virtual void PostDuplicate()
        {
        }
    }
}
