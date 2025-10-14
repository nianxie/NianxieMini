using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Components;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using XLua;

namespace Nianxie.Craft
{
    public class EditRoot: MonoBehaviour, IScrollHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private Camera m_Camera;
        public Camera camera => m_Camera;
        [SerializeField]
        private EditArea m_Area;
        public EditArea area => m_Area;

        public AbstractAssetSlot selectAssetSlot;
        public PositionSlot selectPosSlot;

        private CraftModule craftModule;
        private SlotBehaviour slotRoot;
        public void Init(CraftModule _craftModule, SlotBehaviour _slotRoot)
        {
            craftModule = _craftModule;
            slotRoot = _slotRoot;
            foreach (var slotCom in slotRoot.GetComponentsInChildren<AbstractSlotCom>())
            {
                slotCom.editRoot = this;
            }
        }

        public void OnSelect(AbstractAssetSlot assetSlot)
        {
            if (assetSlot == null)
            {
                selectAssetSlot = null;
                selectPosSlot = null;
                craftModule.editArgs.onSelect?.Action(false);
            }
            else
            {
                selectAssetSlot = assetSlot;
                selectPosSlot = assetSlot.GetComponentInParent<PositionSlot>();
                craftModule.editArgs.onSelect?.Action(assetSlot);
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            //Debug.Log($"initialize {eventData.pointerId}");
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log($"begin drag {eventData.pointerId}");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log($"end drag {eventData.pointerId}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            var delta = eventData.delta;
            camera.transform.position -= camera.ScreenToWorldPoint(delta) - camera.ScreenToWorldPoint(Vector3.zero);
        }

        public void OnScroll(PointerEventData eventData)
        {
            var center = eventData.position;
            var deltaY = eventData.scrollDelta.y;
            var curPinch = camera.ScreenToWorldPoint(center);
            camera.orthographicSize = Mathf.Max(0.5f, camera.orthographicSize - deltaY*0.001f);
            var newPinch = camera.ScreenToWorldPoint(center);
            camera.transform.position = camera.transform.position - newPinch + curPinch;
        }
    }
}