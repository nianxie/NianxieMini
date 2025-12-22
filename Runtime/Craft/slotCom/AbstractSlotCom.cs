using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotCom:MonoBehaviour, IUnionSlot
    {
        public SlotCallback slotCallback => slotInjected.behav.slotCallback;

        public SlotInjected slotInjected { get; private set; }

        public void Init(SlotInjected injected)
        {
            slotInjected = injected;
        }

        public abstract AbstractSlotJson PackToJson(AbstractPackContext packContext);
        public abstract void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson);

        public virtual object GetValue()
        {
            throw new NotImplementedException("TODO");
        }

        public virtual void SetValue(object o)
        {
            throw new NotImplementedException("TODO");
        }

        protected virtual void Awake()
        {
            var pos = transform.localPosition;
            transform.localPosition = new Vector3(pos.x, pos.y, -0.1f);
        }

#if UNITY_EDITOR
        [BlackList]
        public virtual void EditorInspectorUpdate(bool change)
        {
        }
        [BlackList]
        public virtual void EditorLocalUpdate()
        {
        }
        protected virtual void OnValidate()
        {
        }
#endif
    }
}