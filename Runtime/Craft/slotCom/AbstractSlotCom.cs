using System;
using Nianxie.Utils;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotCom<TSlotTarget, TSlotJson>:AbstractSlotCom
    {
        public sealed override AbstractSlotJson PackToJson(IPutAsset putAsset)
        {
            return null;
        }

    }

    public abstract class AbstractSlotCom:MonoBehaviour, IUnionSlot
    {
        public SlotCallback slotCallback => slotInjected.behav.slotCallback;

        public SlotInjected slotInjected { get; private set; }

        public void Init(SlotInjected injected)
        {
            slotInjected = injected;
        }

        public abstract AbstractSlotJson PackToJson(IPutAsset putAsset);
        public abstract void UnpackFromJson(IGetAsset getAsset, AbstractSlotJson slotJson);

        public abstract void AssignValue(object o);

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