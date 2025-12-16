using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using XLua;

namespace Nianxie.Craft
{
    [DisallowMultipleComponent]
    public class PositionSlot:AbstractSlotCom
    {
        [SlotValue]
        [SerializeField]
        private SlotValue<Vector2> m_SlotValue;

        [BlackList]
        public override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            var pos = transform.localPosition;
            return new PositionJson()
            {
                x=pos.x,
                y=pos.y,
            };
        }

        public override object slotValue {
            get => m_SlotValue.ReadValue();
            set => m_SlotValue.AssignValue((Vector2)value);
        }

        [BlackList]
        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            var posJson = (PositionJson)slotJson;
            transform.localPosition = new Vector3(posJson.x, posJson.y, -0.01f);
        }
#if UNITY_EDITOR
        [BlackList]
        public override void ON_INSPECTOR_UPDATE(bool change)
        {
            m_SlotValue.defaultValue = transform.localPosition;
        }
#endif
    }
}