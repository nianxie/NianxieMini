using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SlotSelectHead))]
    [DisallowMultipleComponent]
    public class PositionSlot:AbstractSlotCom<Vector2, PositionJson>
    {
        protected override PositionJson TypedPackToJson()
        {
            var pos = transform.localPosition;
            return new PositionJson()
            {
                x=pos.x,
                y=pos.y,
            };
        }
        
        protected override void TypedUnpackFromJson(PositionJson posJson)
        {
            transform.localPosition = new Vector3(posJson.x, posJson.y, -0.01f);
        }
    }
}