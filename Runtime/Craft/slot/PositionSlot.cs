using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using XLua;

namespace Nianxie.Craft
{
    public class PositionSlot:AbstractSlotCom, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private Vector2 m_DefaultPosition;
        
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

        public Vector2 ReadPosition()
        {
            return transform.localPosition;
        }

        public override object ReadData()
        {
            return ReadPosition();
        }

        [BlackList]
        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            var posJson = (PositionJson)slotJson;
            transform.localPosition = new Vector3(posJson.x, posJson.y, -0.01f);
        }
        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            craftModule.DispatchSlotDrag(this, nameof(IInitializePotentialDragHandler.OnInitializePotentialDrag), eventData);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            craftModule.DispatchSlotDrag(this, nameof(IBeginDragHandler.OnBeginDrag), eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            craftModule.DispatchSlotDrag(this, nameof(IEndDragHandler.OnEndDrag), eventData);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            craftModule.DispatchSlotDrag(this, nameof(IDragHandler.OnDrag), eventData);
        }
#if UNITY_EDITOR
        [BlackList]
        public override void OnInspectorUpdate(bool change)
        {
            m_DefaultPosition = transform.localPosition;
        }
#endif
    }
}