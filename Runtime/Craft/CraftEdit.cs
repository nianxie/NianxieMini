using System;
using System.Collections.Generic;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

namespace Nianxie.Craft
{
    public class CraftEdit: SlotCallback, IScrollHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
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
        [SerializeField]
        private CanvasScaler m_CanvasScaler;

        public SlotBehaviour rootSlot { get; private set; }

        private void InitByLoading(LuafabLoading miniCraftLoading)
        {
            var miniBehav = miniCraftLoading.RawFork(editArea.transform);
            if (!miniBehav.TryGetComponent<SlotBehaviour>(out var behavSlot))
            {
                throw new Exception("BehavSlot expected in root of MiniCraft");
            }
            rootSlot = behavSlot;
            foreach (var slotCom in GetComponentsInChildren<AbstractSlotCom>(true))
            {
                slotCom.slotCallback = this;
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
            editCamera.transform.position -= editCamera.ScreenToWorldPoint(delta) - editCamera.ScreenToWorldPoint(Vector3.zero);
            ShellRefresh();
        }

        public void OnScroll(PointerEventData eventData)
        {
            var center = eventData.position;
            var deltaY = eventData.scrollDelta.y;
            var curPinch = editCamera.ScreenToWorldPoint(center);
            editCamera.orthographicSize = Mathf.Max(0.5f, editCamera.orthographicSize - deltaY*0.001f);
            var newPinch = editCamera.ScreenToWorldPoint(center);
            editCamera.transform.position = editCamera.transform.position - newPinch + curPinch;
            ShellRefresh();
        }

        [BlackList]
        public void PlayMain(MiniGameManager gameManager, LuafabLoading miniCraftLoading)
        {
            editCamera.gameObject.SetActive(false);
            editCanvas.gameObject.SetActive(false);
            reflectEnv = gameManager.reflectEnv;
            if (miniCraftLoading != null)
            {
                InitByLoading(miniCraftLoading);
                var craftJson = gameManager.playArgs.craftJson;
                var atlasTex = gameManager.playArgs.atlasTex;
                if (craftJson != null)
                {
                    var unpackContext = new CraftUnpackContext(craftJson, atlasTex);
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

        [BlackList]
        public void EditMain(MiniEditArgs args, LuafabLoading miniCraftLoading)
        {
            editArgs = args;
            editCamera.gameObject.SetActive(true);
            InitByLoading(miniCraftLoading);
        }

        /// <summary>
        /// 获取nodeSlot在屏幕空间的矩形.
        /// </summary>
        /// <param name="nodeSlot"></param>
        /// <returns></returns>
        public Rect ToCanvasRect(AbstractNodeSlot nodeSlot)
        {
            // 1. 获取RectTransform的四个角点（本地空间，顺序：左下、左上、右上、右下）
            Vector3[] corners = new Vector3[4];
            nodeSlot.rectTransform.GetWorldCorners(corners);

            // 2. 将四个角点转换为屏幕空间（像素坐标）
            for (int i = 0; i < 4; i++)
            {
                // RectTransform的世界坐标转屏幕坐标（Screen空间：原点左上，Y向下）
                corners[i] = RectTransformUtility.WorldToScreenPoint(editCamera, corners[i]);
            }

            // 3. 计算屏幕空间的Rect边界
            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            // 4. 构建屏幕空间的Rect（Screen空间Y轴向下，所以y取minY，height=maxY-minY）
            Rect screenRect = new Rect();
            screenRect.x = minX;
            screenRect.y = minY;
            screenRect.width = maxX - minX;
            screenRect.height = maxY - minY;

            // 5. 适配Canvas Scaler
            var scaleFactor = 1.0f;
            switch (m_CanvasScaler.uiScaleMode)
            {
                case CanvasScaler.ScaleMode.ConstantPixelSize:
                {
                    // 固定像素尺寸：缩放因子=1（若设置了scaleFactor则乘以该值）
                    scaleFactor = m_CanvasScaler.scaleFactor;
                    break;
                }
                case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                {
                    // 随屏幕尺寸缩放：计算基于参考分辨率的缩放因子
                    Vector2 screenSize = new Vector2(Screen.width, Screen.height);
                    Vector2 refSize = m_CanvasScaler.referenceResolution;

                    if (refSize.x == 0 || refSize.y == 0)
                    {
                        scaleFactor = 1.0f;
                    }
                    else
                    {
                        float scaleX = screenSize.x / refSize.x;
                        float scaleY = screenSize.y / refSize.y;

                        switch (m_CanvasScaler.screenMatchMode)
                        {
                            case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                                // 按宽高匹配比例（match=0→匹配宽，match=1→匹配高）
                                scaleFactor = Mathf.Lerp(scaleX, scaleY, m_CanvasScaler.matchWidthOrHeight) *
                                       m_CanvasScaler.scaleFactor;
                                break;
                            case CanvasScaler.ScreenMatchMode.Expand:
                                // 扩展：取较小的缩放因子（保证UI完整显示）
                                scaleFactor = Mathf.Min(scaleX, scaleY) * m_CanvasScaler.scaleFactor;
                                break;
                            case CanvasScaler.ScreenMatchMode.Shrink:
                                // 收缩：取较大的缩放因子（保证UI填满屏幕）
                                scaleFactor = Mathf.Max(scaleX, scaleY) * m_CanvasScaler.scaleFactor;
                                break;
                        }
                    }
                    break;
                }
                case CanvasScaler.ScaleMode.ConstantPhysicalSize:
                {
                    // 固定物理尺寸：基于屏幕DPI计算
                    float dpi = Screen.dpi > 0 ? Screen.dpi : m_CanvasScaler.fallbackScreenDPI;
                    float physicalScale = dpi / m_CanvasScaler.referencePixelsPerUnit;
                    scaleFactor = physicalScale * m_CanvasScaler.scaleFactor;
                    break;
                }
            }
            screenRect = new Rect(
                screenRect.x / scaleFactor,
                screenRect.y / scaleFactor,
                screenRect.width / scaleFactor,
                screenRect.height / scaleFactor 
            );
            return screenRect;
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