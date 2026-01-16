using System;
using Nianxie.Utils;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotCom<TSlotTarget, TSlotJson>:AbstractSlotCom, IUnionSlot where TSlotJson:AbstractSlotJson<TSlotTarget>
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

    public abstract class AbstractSlotCom:MonoBehaviour
    {
        public ISlotHandler slotHandler => slotInjected.behav.slotHandler;
        public SlotInjected slotInjected { get; private set; }

        public void Init(SlotInjected injected)
        {
            slotInjected = injected;
        }

        /*AbstractSlotJson IUnionSlot.PackToJson(IPutAsset putAsset)
        {
            throw new NotImplementedException($"{nameof(IUnionSlot.PackToJson)} not implement");
        }

        void IUnionSlot.UnpackFromJson(IGetAsset getAsset, AbstractSlotJson slotJson)
        {
            throw new NotImplementedException($"{nameof(IUnionSlot.UnpackFromJson)} not implement");
        }*/
        
        protected virtual void Awake()
        {
            var pos = transform.localPosition;
            transform.localPosition = new Vector3(pos.x, pos.y, -0.1f);
        }

#if UNITY_EDITOR
        [BlackList]
        public virtual void EditorInspectorUpdate(NianxieDefaultAssets defaultAssets)
        {
        }
        [BlackList]
        public virtual void EditorLocalUpdate(NianxieDefaultAssets defaultAssets)
        {
        }
        protected virtual void OnValidate()
        {
        }
#endif
    }
}