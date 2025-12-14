using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotCom:MonoBehaviour, IUnionSlot
    {
        public SlotCallback slotCallback => slotField.behav.slotCallback;

        public SlotField slotField { get; private set; }

        public void Init(SlotField field)
        {
            slotField = field;
        }

        public abstract AbstractSlotJson PackToJson(AbstractPackContext packContext);
        public abstract void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson);

        public virtual object ReadData()
        {
            throw new NotImplementedException();
        }

        protected virtual void Awake()
        {
            var pos = transform.localPosition;
            transform.localPosition = new Vector3(pos.x, pos.y, -0.1f);
        }

#if UNITY_EDITOR
        [BlackList]
        public virtual void ON_INSPECTOR_UPDATE(bool change)
        {
        }
#endif
    }
}