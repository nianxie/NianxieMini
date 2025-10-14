using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using XLua;

namespace Nianxie.Craft
{
    [DisallowMultipleComponent]
    public class PositionSlot:AbstractSlotCom, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private Vector2 m_DefaultPosition;

        public bool dragging { get; private set; }

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
            if (editRoot.selectPosSlot == this)
            {
                eventData.useDragThreshold = false;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
            if (editRoot.selectPosSlot != this)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
            if (editRoot.selectPosSlot != this)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
            }
            Debug.Log("position slot end drag");
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (editRoot.selectPosSlot != this)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
            }
            else
            {
                var delta = eventData.delta;
                transform.position += editRoot.camera.ScreenToWorldPoint(delta) - editRoot.camera.ScreenToWorldPoint(Vector3.zero);
            }
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