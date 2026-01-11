using System;
using UnityEngine;

namespace Nianxie.Craft
{
    public abstract class AbstractRenderSlot<TSlotTarget, TSlotJson>:AbstractRenderSlot, IUnionSlot where TSlotJson:AbstractSlotJson<TSlotTarget>
    {
        [SerializeField]
        protected SlotValue<TSlotTarget> m_SlotValue;
        
        AbstractSlotJson IUnionSlot.RawPack(IPutAsset putAsset)
        {
            return PackToJson(putAsset);
        }

        void IUnionSlot.RawUnpack(IGetAsset getAsset, AbstractSlotJson slotJson)
        {
            UnpackFromJson(getAsset, slotJson as TSlotJson);
        }
        protected abstract TSlotJson PackToJson(IPutAsset putAsset);
        protected abstract void UnpackFromJson(IGetAsset getAsset, TSlotJson slotJson);
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
