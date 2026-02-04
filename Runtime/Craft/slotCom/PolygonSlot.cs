using UnityEngine;
using System;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SpriteSlot))]
    [DisallowMultipleComponent]
    public class PolygonSlot:AbstractSlotCom<SpritePolygon, PolygonJson>
    {
        private Sprite syncSprite;

        public PolygonCollider2D targetPolygonCollider;

        public void CalculatePolygon(Sprite targetSprite)
        {
            var tex = targetSprite.texture;
            if (tex.width >= 2 && tex.height >= 2)
            {
                var paths = ContourTracer.CalcPolygon(tex.GetPixels32(), new Vector2Int(tex.width, tex.height), new Vector2(targetSprite.pivot.x/(tex.width-1),targetSprite.pivot.y/(tex.height-1)), targetSprite.pixelsPerUnit);
                var spritePolygon = new SpritePolygon(paths);
                if (targetPolygonCollider != null)
                {
                    spritePolygon.ApplyTo(targetPolygonCollider);
                }
            }
        }

        protected override PolygonJson TypedPackToJson()
        {
            throw new NotImplementedException("pack polygon TODO");
        }
        
        protected override void TypedUnpackFromJson(PolygonJson slotJson)
        {
            throw new NotImplementedException("unpack polygon TODO");
        }
        
        public override void AssignValue(SpritePolygon vec)
        {
            m_SlotValue.Set(vec);
        }
#if UNITY_EDITOR
#endif
    }
}