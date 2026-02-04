using System;
using UnityEngine;

namespace Nianxie.Craft
{
    public abstract class AbstractRenderSlot<TSlotTarget, TSlotJson>:AbstractRenderSlot, IUnionSlot where TSlotJson:AbstractSlotJson<TSlotTarget>
    {
        AbstractSlotJson IUnionSlot.PackToJson()
        {
            return TypedPackToJson();
        }

        void IUnionSlot.UnpackFromJson(AbstractSlotJson slotJson)
        {
            TypedUnpackFromJson(slotJson as TSlotJson);
        }
        protected abstract TSlotJson TypedPackToJson();
        protected abstract void TypedUnpackFromJson(TSlotJson slotJson);
    }
    
    [RequireComponent(typeof(SlotSelectHead))]
    [DisallowMultipleComponent]
    public abstract class AbstractRenderSlot : AbstractSlotCom
    {
        [NonSerialized] SlotSelectHead m_SlotSelectHead;
        public SlotSelectHead selectHead
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
        
    }
}
