using System;
using UnityEngine;

namespace Nianxie.Craft
{
    public abstract class AbstractRenderSlot<TSlotTarget, TSlotJson>:AbstractRenderSlot, IUnionSlot where TSlotJson:AbstractSlotJson<TSlotTarget>
    {
        AbstractSlotJson IUnionSlot.PackToJson(IPackContext packContext)
        {
            return TypedPackToJson(packContext);
        }

        void IUnionSlot.UnpackFromJson(UnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            TypedUnpackFromJson(unpackContext, slotJson as TSlotJson);
        }
        protected abstract TSlotJson TypedPackToJson(IPackContext packContext);
        protected abstract void TypedUnpackFromJson(UnpackContext unpackContext, TSlotJson slotJson);
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
