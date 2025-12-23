using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    public class SlotSelectHead: AbstractSlotSelect, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
    {
        [SerializeField] private SlotSelectBody m_SlotSelectBody;
        [BlackList]
        public SlotSelectBody selectBody => m_SlotSelectBody;

        public AbstractRenderSlot renderSlot { get; private set; }
        public SlotBehaviour slotBehav { get; private set; }
        public PositionSlot posSlot { get; private set; }
        private IUnionSlot unionSlot => slotBehav != null ? slotBehav : renderSlot;
        private void Awake()
        {
            slotBehav = GetComponent<SlotBehaviour>();
            renderSlot = GetComponent<AbstractRenderSlot>();
            posSlot = GetComponent<PositionSlot>();
        }
        
        public bool IsList()
        {
            var slotField = unionSlot.slotInjected;
            return slotField != null && slotField.injection.multipleKind == InjectionMultipleKind.List;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            unionSlot.slotCallback.OnSelect(this);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
        }
        public void DuplicateSelf()
        {
            if (IsList())
            {
                var slotField = unionSlot.slotInjected;
                slotField.behav.GetSlotList(slotField.injection).DuplicateElement(this);
            }
        }
        public void DeleteSelf()
        {
            if (IsList())
            {
                var slotField = unionSlot.slotInjected;
                slotField.behav.GetSlotList(slotField.injection).DeleteElement(this);
            }
        }
#if UNITY_EDITOR
        [BlackList]
        public override void EditorInspectorUpdate()
        {
            SlotSelectBody body = null;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out body))
                {
                    break;
                }
            }
            if (m_SlotSelectBody != body)
            {
                m_SlotSelectBody = body;
                UnityEditor.EditorUtility.SetDirty(this);
                if (m_SlotSelectBody == null)
                {
                    Debug.LogError($"{nameof(SlotSelectBody)} require a {nameof(SlotSelectHead)} in parent");
                }
            }

            if (m_SlotSelectBody != null)
            {
                m_SlotSelectBody.EditorLocalUpdate();
            }
            if (TryGetComponent<AbstractRenderSlot>(out var renderSlot))
            {
                renderSlot.EditorLocalUpdate();
            }
        }

        [BlackList]
        public override void EditorLocalUpdate()
        {
        }

        [BlackList]
        public void EnsureSelectBodyInChild()
        {
            if (UnityEditor.EditorUtility.IsPersistent(this)) 
            {
                return; 
            }
            SlotSelectBody body = null;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out body))
                {
                    break;
                }
            }

            if (body == null)
            {
                var bodyGo = new GameObject(SELECT_BODY_NAME, typeof(SlotSelectBody));
                m_SlotSelectBody = bodyGo.GetComponent<SlotSelectBody>();
                bodyGo.transform.parent = transform;
                bodyGo.transform.localPosition = Vector3.zero;
                bodyGo.transform.localRotation = Quaternion.identity;
                bodyGo.transform.SetSiblingIndex(0);
                UnityEditor.Undo.RegisterCreatedObjectUndo(bodyGo, "create select body");
            }
            else
            {
                m_SlotSelectBody = body;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            m_SlotSelectBody.EditorInspectorUpdate();
        }

        [BlackList]
        public void Reset()
        {
            EnsureSelectBodyInChild();
        }
#endif
    }
}