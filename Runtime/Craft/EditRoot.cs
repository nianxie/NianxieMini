using System;
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

        public BehavSlot rootSlot { get; private set; }

        private void InitByLoading(LuafabLoading craftLuafabLoading, bool silent)
        {
            var miniBehav = (MiniBehaviour) craftLuafabLoading.RawFork(area.transform);
            if (!miniBehav.TryGetComponent<BehavSlot>(out var behavSlot))
            {
                throw new Exception("BehavSlot expected in root of MiniCraft");
            }
            rootSlot = behavSlot;
            if (silent)
            {
                foreach (var childRenderer in behavSlot.gameObject.GetComponentsInChildren<Renderer>())
                {
                    childRenderer.enabled = false;
                }

                foreach (var childCollider2D in behavSlot.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    childCollider2D.enabled = false;
                }

                foreach (var childCollider in behavSlot.gameObject.GetComponentsInChildren<Collider>())
                {
                    childCollider.enabled = false;
                }
            }
            foreach (var slotCom in rootSlot.GetComponentsInChildren<AbstractSlotCom>())
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
                editArgs.onSelect?.Action(false);
            }
            else
            {
                selectAssetSlot = assetSlot;
                selectPosSlot = assetSlot.GetComponentInParent<PositionSlot>();
                editArgs.onSelect?.Action(assetSlot);
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

        private MiniEditArgs editArgs;

        public void PlayMain(MiniPlayArgs args, LuafabLoading miniCraftLoading)
        {
            camera.gameObject.SetActive(false);
            if (miniCraftLoading != null)
            {
                InitByLoading(miniCraftLoading, true);

                var craftJson = args.craftJson;
                var altasTex = args.atlasTex;
                if (craftJson != null)
                {
                    var unpackContext = new CraftUnpackContext(craftJson, altasTex);
                    unpackContext.UnpackRoot(rootSlot);
                }
            }
        }

        public void EditMain(MiniEditArgs args, LuafabLoading miniCraftLoading)
        {
            editArgs = args;
            camera.gameObject.SetActive(true);
            InitByLoading(miniCraftLoading, false);
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