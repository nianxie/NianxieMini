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

        public override object GetValue()
        {
            return m_SlotValue.ReadValue();
        }
        
        public override void SetValue(object o)
        {
            var vec = m_SlotValue.SafeCast(o);
            m_SlotValue.AssignValue(vec);
        }

        [BlackList]
        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            var posJson = (PositionJson)slotJson;
            transform.localPosition = new Vector3(posJson.x, posJson.y, -0.01f);
        }
#if UNITY_EDITOR
        [BlackList]
        public override void EditorInspectorUpdate(bool change)
        {
            m_SlotValue.defaultValue = transform.localPosition;
        }
#endif
    }
}