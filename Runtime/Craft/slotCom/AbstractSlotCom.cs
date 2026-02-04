using System;
using System.Collections.Generic;
using Nianxie.Utils;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotCom<TSlotTarget, TSlotJson>:AbstractSlotCom, IUnionSlot where TSlotJson:AbstractSlotJson<TSlotTarget>
    {
        [SerializeField]
        protected SlotValue<TSlotTarget> m_SlotValue;
        
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
        public abstract void AssignValue(TSlotTarget o);
    }

    public abstract class AbstractSlotCom:MonoBehaviour, IUnionSlot
    {
        public ISlotHandler slotHandler => slotInjected.behav.slotHandler;
        public SlotInjected slotInjected { get; private set; }

        public virtual void Init(SlotInjected injected)
        {
            slotInjected = injected;
        }

        AbstractSlotJson IUnionSlot.PackToJson()
        {
            throw new NotImplementedException($"{nameof(IUnionSlot.PackToJson)} not implement");
        }

        void IUnionSlot.UnpackFromJson(AbstractSlotJson slotJson)
        {
            throw new NotImplementedException($"{nameof(IUnionSlot.UnpackFromJson)} not implement");
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