using UnityEngine;

namespace Nianxie.Craft
{
    // IUnionSlot要么是SlotBehaviour要么是AbstractSlotCom
    public interface IUnionSlot
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public void Init(SlotField field);
        public SlotField slotField { get; }
        public SlotCallback slotCallback { get; }
        public object ReadData();
        public AbstractSlotJson PackToJson(AbstractPackContext packContext);
        public void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson);
    }
}