using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Riff;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

namespace Nianxie.Craft
{
    public struct MiniEditArgs
    {
        public LuaFunction shellRefresh;
        public LuaFunction shellRelease;
    }
    public class CraftManager: ICraftManager, ISlotHandler, IScrollHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
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
        
        [SerializeField]
        private MiniGameManager manager;

        [SerializeField] 
        private AssetUsageCenter m_AssetUsageCenter;
        public AssetUsageCenter assetUsageCenter => m_AssetUsageCenter;
        public SlotSelectHead slotSelect { get; private set; }
        
        private SlotBehaviour rootSlot;
        
        [BlackList]
        public MiniEditArgs editArgs;

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            //Debug.Log($"initialize {eventData.pointerId}");
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log($"begin drag {eventData.pointerId}");
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log($"end drag {eventData.pointerId}");
        }

        /// <summary>
        /// 在EditArea中响应OnClick，用来实现选中取消，Drag的事件响应放到这一级，从而避免drag时也触发OnClick
        /// </summary>
        /// <param name="eventData"></param>
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var delta = eventData.delta;
            editCamera.transform.position -= editCamera.ScreenToWorldPoint(delta) - editCamera.ScreenToWorldPoint(Vector3.zero);
            (this as ISlotHandler).ShellRefresh();
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            var center = eventData.position;
            var deltaY = eventData.scrollDelta.y;
            var curPinch = editCamera.ScreenToWorldPoint(center);
            editCamera.orthographicSize = Mathf.Max(0.5f, editCamera.orthographicSize - deltaY*0.001f);
            var newPinch = editCamera.ScreenToWorldPoint(center);
            editCamera.transform.position = editCamera.transform.position - newPinch + curPinch;
            (this as ISlotHandler).ShellRefresh();
        }

        /// <summary>
        /// 获取slotSelect在屏幕空间的矩形.
        /// </summary>
        /// <param name="selectHead"></param>
        /// <returns></returns>
        public Rect ToCanvasRect(SlotSelectHead selectHead)
        {
            // 1. 获取bounds
            var bounds = selectHead.selectBody.touchCollider2D.bounds;
            
            var minPos = RectTransformUtility.WorldToScreenPoint(editCamera, bounds.min);
            var maxPos = RectTransformUtility.WorldToScreenPoint(editCamera, bounds.max);

            // 2. 计算屏幕空间的bounds的边界
            float minX = Mathf.Min(minPos.x, maxPos.x);
            float maxX = Mathf.Max(minPos.x, maxPos.x);
            float minY = Mathf.Min(minPos.y, maxPos.y);
            float maxY = Mathf.Max(minPos.y, maxPos.y);

            // 3. 构建屏幕空间的Rect（Screen空间Y轴向下，所以y取minY，height=maxY-minY）
            Rect screenRect = new Rect();
            screenRect.x = minX;
            screenRect.y = minY;
            screenRect.width = maxX - minX;
            screenRect.height = maxY - minY;

            // 4. 适配Canvas Scaler
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
        
        public async UniTask<byte[]> PackCraftAsync<T>()
        {
            var ctx = new PackContext(assetUsageCenter);
            return await ctx.PackRoot(rootSlot);
        }
        
        #region ICraftManager
        private async UniTask InitRootSlot()
        {
            // Instantiate MiniCraft as rootSlot
            var miniCraftLuafab = manager.GetComponent<AssetModule>().AttachLuafabLoading(manager.bridge.envPaths.miniCraftLuafabPath, false);
            await miniCraftLuafab.WaitTask;
            var behav = miniCraftLuafab.RawFork(editArea.transform);
            if (behav is SlotBehaviour slotBehav)
            {
                rootSlot = slotBehav;
                rootSlot.RootInit(this);
            }
            else
            {
                throw new System.Exception("BehavSlot expected in root of MiniCraft");
            }
        }

        [BlackList]
        public override async UniTask<LuaTable> PlayCraftTable()
        {
            if (manager.bridge.miniConfig.craftable)
            {
                assetUsageCenter.Main();
                var riffPackage = manager.bridge.riffPackage;
                await InitRootSlot();
                if (riffPackage == null)
                {
                    var unpackContext = new UnpackContext(assetUsageCenter, manager.reflectEnv);
                    var defaultPackContext = new DefaultPackContext();
                    var rootJson = rootSlot.TypedPackToJson(defaultPackContext);
                    return rootJson.Export(unpackContext);
                }
                else
                {
                    var unpackContext = new UnpackContext(assetUsageCenter, manager.reflectEnv);
                    var rootJson = (riffPackage.custom as CraftRiffJson).root;
                    return rootJson.Export(unpackContext);
                }
            }
            return null;
        }

        [BlackList]
        public async UniTask EditMain(MiniEditArgs args)
        {
            editArgs = args;
            assetUsageCenter.Main();
            await InitRootSlot();
            // unpack from root slot
            var riffPackage = manager.bridge.riffPackage;
            if (riffPackage != null)
            {
                var unpackContext = new UnpackContext(assetUsageCenter, manager.reflectEnv);
                var rootJson = (riffPackage.custom as CraftRiffJson).root;
                rootSlot.TypedUnpackFromJson(unpackContext, rootJson);
            }
        }
        #endregion

        #region ISlotHandler

        void ISlotHandler.ShellRefresh()
        {
            editArgs.shellRefresh.Action();
        }
        void ISlotHandler.OnSelect(SlotSelectHead slot)
        {
            if (slot == null)
            {
                slotSelect = null;
            }
            else
            {
                slotSelect = slot;
            }
            (this as ISlotHandler).ShellRefresh();
        }

        void ISlotHandler.RegisterBuiltinObject(string builtinPath, UnityEngine.Object builtinObj)
        {
            assetUsageCenter.RegisterBuiltinObject(builtinPath, builtinObj);
        }
        bool ISlotHandler.IsBuiltinObject(UnityEngine.Object builtinObj, out string builtinPath)
        {
            return assetUsageCenter.IsBuiltinObject(builtinObj, out builtinPath);
        }
        #endregion
    }
}