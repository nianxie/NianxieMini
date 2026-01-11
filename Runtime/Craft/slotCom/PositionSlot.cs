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
    public class PositionSlot:AbstractSlotCom<Vector2, PositionJson>
    {
        protected override PositionJson PackToJson(IPutAsset putAsset)
        {
            var pos = transform.localPosition;
            return new PositionJson()
            {
                x=pos.x,
                y=pos.y,
            };
        }
        
        protected override void UnpackFromJson(IGetAsset getAsset, PositionJson slotJson)
        {
            var posJson = (PositionJson)slotJson;
            transform.localPosition = new Vector3(posJson.x, posJson.y, -0.01f);
        }
        
        public override void AssignValue(Vector2 vec)
        {
            m_SlotValue.Set(vec);
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