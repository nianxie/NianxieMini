using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    public class PositionSlot:AbstractSlotCom<PositionJson, Vector3, Vector3>, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        protected override PositionJson PackFromSource(AbstractPackContext packContext, Vector3 source)
        {
            return new PositionJson
            {
                x = source.x,
                y = source.y,
                z = source.z,
            };
        }

        protected override Vector3 UnpackToTarget(CraftUnpackContext unpackContext, PositionJson slotJson)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnChangeValue()
        {
            throw new System.NotImplementedException();
        }

        protected override Vector3 ValueProcess(Vector3 source)
        {
            return source;
        }
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            craftModule.DispatchSlotDrag(this, nameof(OnInitializePotentialDrag), eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            craftModule.DispatchSlotDrag(this, nameof(OnBeginDrag), eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            craftModule.DispatchSlotDrag(this, nameof(OnEndDrag), eventData);
        }
        public void OnDrag(PointerEventData eventData)
        {
            craftModule.DispatchSlotDrag(this, nameof(OnDrag), eventData);
        }
    }
}