using System;
using UnityEngine;

namespace Nianxie.Craft
{
    public abstract class AbstractRenderSlot<TSlotTarget, TSlotJson>:AbstractRenderSlot, IUnionSlot where TSlotJson:AbstractSlotJson<TSlotTarget>
    {
        [SerializeField]
        protected SlotValue<TSlotTarget> m_SlotValue;
        
        AbstractSlotJson IUnionSlot.PackToJson(IPutAsset putAsset)
        {
            return TypedPackToJson(putAsset);
        }

        void IUnionSlot.UnpackFromJson(IGetAsset getAsset, AbstractSlotJson slotJson)
        {
            TypedUnpackFromJson(getAsset, slotJson as TSlotJson);
        }
        protected abstract TSlotJson TypedPackToJson(IPutAsset putAsset);
        protected abstract void TypedUnpackFromJson(IGetAsset getAsset, TSlotJson slotJson);
        public abstract void AssignValue(TSlotTarget o);
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
