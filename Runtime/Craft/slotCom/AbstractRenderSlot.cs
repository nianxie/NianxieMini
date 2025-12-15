using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SlotSelectable))]
    [DisallowMultipleComponent]
    public abstract class AbstractRenderSlot : AbstractSlotCom
    {
        [NonSerialized] SlotSelectable m_SlotSelectable;
        public SlotSelectable selectable
        {
            get
            {
                if (!m_SlotSelectable)
                {
                    TryGetComponent(out m_SlotSelectable);
                }
                return m_SlotSelectable;
            }
        }
        
        public virtual void PostDuplicate()
        {
        }
    }
}
