using System;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nianxie.Craft
{
    public class CraftEdit: MonoBehaviour, IScrollHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private Camera m_Camera;
        public Camera editCamera => m_Camera;
        [SerializeField]
        private EditArea m_Area;
        public EditArea editArea => m_Area;
        [SerializeField]
        private Canvas m_Canvas;
        public Canvas editCanvas => m_Canvas;

        public AbstractNodeSlot selectNodeSlot { get; private set; }
        public PositionSlot selectPosSlot { get; private set; }
        public BehavSlot rootSlot { get; private set; }

        private void InitByLoading(LuafabLoading miniCraftLoading)
        {
            var miniBehav = miniCraftLoading.RawFork(editArea.transform);
            if (!miniBehav.TryGetComponent<BehavSlot>(out var behavSlot))
            {
                throw new Exception("BehavSlot expected in root of MiniCraft");
            }
            rootSlot = behavSlot;
            foreach (var slotCom in GetComponentsInChildren<AbstractSlotCom>(true))
            {
                slotCom.craftEdit = this;
            }
        }

        public void OnGizmosRefresh()
        {
            editArgs.refresh.Action();
        }

        public void OnSelect(AbstractNodeSlot assetSlot)
        {
            if (assetSlot == null)
            {
                selectNodeSlot = null;
                selectPosSlot = null;
            }
            else
            {
                selectNodeSlot = assetSlot;
                selectPosSlot = assetSlot.GetComponentInParent<PositionSlot>();
            }
            OnGizmosRefresh();
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
            editCamera.transform.position -= editCamera.ScreenToWorldPoint(delta) - editCamera.ScreenToWorldPoint(Vector3.zero);
            OnGizmosRefresh();
        }

        public void OnScroll(PointerEventData eventData)
        {
            var center = eventData.position;
            var deltaY = eventData.scrollDelta.y;
            var curPinch = editCamera.ScreenToWorldPoint(center);
            editCamera.orthographicSize = Mathf.Max(0.5f, editCamera.orthographicSize - deltaY*0.001f);
            var newPinch = editCamera.ScreenToWorldPoint(center);
            editCamera.transform.position = editCamera.transform.position - newPinch + curPinch;
            OnGizmosRefresh();
        }

        private MiniEditArgs editArgs;

        public void PlayMain(MiniPlayArgs args, LuafabLoading miniCraftLoading)
        {
            editCamera.gameObject.SetActive(false);
            editCanvas.gameObject.SetActive(false);
            if (miniCraftLoading != null)
            {
                InitByLoading(miniCraftLoading);

                var craftJson = args.craftJson;
                var altasTex = args.atlasTex;
                if (craftJson != null)
                {
                    var unpackContext = new CraftUnpackContext(craftJson, altasTex);
                    unpackContext.UnpackRoot(rootSlot);
                }
            }
            foreach (var childRenderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                childRenderer.enabled = false;
            }

            foreach (var childCollider2D in gameObject.GetComponentsInChildren<Collider2D>())
            {
                childCollider2D.enabled = false;
            }

            foreach (var childCollider in gameObject.GetComponentsInChildren<Collider>())
            {
                childCollider.enabled = false;
            }
        }

        public void EditMain(MiniEditArgs args, LuafabLoading miniCraftLoading)
        {
            editArgs = args;
            editCamera.gameObject.SetActive(true);
            InitByLoading(miniCraftLoading);
        }
        
        public (LargeBytes, byte[]) PackJsonPng()
        {
            var packContext = new PngPackContext();
            packContext.PackRoot(rootSlot);
            return packContext.DumpJsonPng();
        }

        private Texture2D editorTex;
        public void UnpackJsonPng(LargeBytes jsonBytes, byte[] pngData)
        {
            if (editorTex != null)
            {
                DestroyImmediate(editorTex);
            }
            var json = CraftJson.FromLargeBytes(jsonBytes);
            editorTex = new Texture2D(2,2);
            editorTex.LoadImage(pngData);
            var context = new CraftUnpackContext(json, editorTex);
            context.UnpackRoot(rootSlot);
        }
    }
}