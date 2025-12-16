using System;
using UnityEngine;

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
                    m_SlotSelectable = GetComponent<SlotSelectable>();
                }
                return m_SlotSelectable;
            }
        }
        
        public virtual void PostDuplicate()
        {
        }
    }
}
