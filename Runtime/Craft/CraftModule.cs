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
    public class CraftModule : AbstractGameModule, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public Camera editCamera;
        public SlotBehaviour craftSlot { get; private set; }
        private MiniGameManager miniManager => (MiniGameManager) gameManager;
        private MiniEditArgs editArgs;
        private MiniPlayArgs playArgs;

        public async UniTask PlayMain(MiniPlayArgs args)
        {
            playArgs = args;
            editCamera.gameObject.SetActive(false);
            if (miniManager.playArgs.craft)
            {
                var craftLuafabLoading =
                    miniManager.assetModule.AttachLuafabLoading(miniManager.bridge.envPaths.miniCraftLuafabPath, false);
                await craftLuafabLoading.WaitTask;
                var slotRoot = (SlotBehaviour) craftLuafabLoading.RawFork(transform);
                foreach (var childRenderer in slotRoot.gameObject.GetComponentsInChildren<Renderer>())
                {
                    childRenderer.enabled = false;
                }

                foreach (var childCollider2D in slotRoot.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    childCollider2D.enabled = false;
                }

                foreach (var childCollider in slotRoot.gameObject.GetComponentsInChildren<Collider>())
                {
                    childCollider.enabled = false;
                }

                var craftJson = miniManager.playArgs.craftJson;
                var altasTex = miniManager.playArgs.atlasTex;
                if (craftJson != null)
                {
                    var unpackContext = new CraftUnpackContext(craftJson, altasTex);
                    unpackContext.UnpackRoot(slotRoot);
                }

                craftSlot = slotRoot;
            }
        }

        public async UniTask EditMain(MiniEditArgs args)
        {
            editArgs = args;
            editCamera.gameObject.SetActive(true);
            var craftLuafabLoading =
                miniManager.assetModule.AttachLuafabLoading(miniManager.bridge.envPaths.miniCraftLuafabPath, false);
            await craftLuafabLoading.WaitTask;
            craftSlot = (SlotBehaviour) craftLuafabLoading.RawFork(transform);
            foreach (var com in craftSlot.GetComponentsInChildren<AbstractSlotCom>(true))
            {
                com.craftModule = this;
            }
        }

        public void DispatchSlotDrag(PositionSlot posSlot, string name, PointerEventData evt)
        {
            editArgs.dispatchDrag?.Action(posSlot, name, evt);
        }
        
        public void DispatchSlotPointer(AbstractSlotCom slotCom, string name, PointerEventData evt)
        {
            editArgs.dispatchPointer?.Action(slotCom, name, evt);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            editArgs.dispatchRootDrag?.Action(nameof(OnInitializePotentialDrag), eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            editArgs.dispatchRootDrag?.Action(nameof(OnBeginDrag), eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            editArgs.dispatchRootDrag?.Action(nameof(OnEndDrag), eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            editArgs.dispatchRootDrag?.Action(nameof(OnDrag), eventData);
        }
        
        public (LargeBytes, byte[]) PackJsonPng()
        {
            var packContext = new PngPackContext();
            packContext.PackRoot(craftSlot);
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
            context.UnpackRoot(craftSlot);
        }

    }
}
