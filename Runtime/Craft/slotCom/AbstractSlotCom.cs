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

    public abstract class AbstractSlotCom:MonoBehaviour
    {
        public SlotCallback slotCallback => slotInjected.behav.slotCallback;
        public SlotInjected slotInjected { get; private set; }

        public void Init(SlotInjected injected)
        {
            slotInjected = injected;
        }

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