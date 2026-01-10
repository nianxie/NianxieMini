using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SlotSelectHead))]
    [DisallowMultipleComponent]
    public class PositionSlot:AbstractSlotCom
    {
        [SerializeField]
        private SlotValue<Vector2> m_SlotValue;

        [BlackList]
        public override AbstractSlotJson PackToJson(IPutAsset putAsset)
        {
            var pos = transform.localPosition;
            return new PositionJson()
            {
                x=pos.x,
                y=pos.y,
            };
        }
        
        public override void AssignValue(object o)
        {
            var vec = m_SlotValue.SafeCast(o);
            m_SlotValue.AssignValue(vec);
        }

        [BlackList]
        public override void UnpackFromJson(IGetAsset getAsset, AbstractSlotJson slotJson)
        {
            var posJson = (PositionJson)slotJson;
            transform.localPosition = new Vector3(posJson.x, posJson.y, -0.01f);
        }
#if UNITY_EDITOR
        [BlackList]
        public override void EditorInspectorUpdate(NianxieDefaultAssets defaultAssets)
        {
            m_SlotValue.defaultValue = transform.localPosition;
        }
#endif
    }
}