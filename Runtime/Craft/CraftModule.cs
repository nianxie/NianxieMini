using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Components;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nianxie.Craft
{
    public class CraftModule : AbstractGameModule
    {
        public Camera editCamera;
        public SlotBehaviour craftSlot { get; private set; }
        private MiniGameManager miniManager => (MiniGameManager) gameManager;

        public async UniTask PlayMain()
        {
            editCamera.gameObject.SetActive(false);
            if (miniManager.args.craft)
            {
                var craftLuafabLoading = miniManager.assetModule.AttachLuafabLoading(miniManager.bridge.envPaths.miniCraftLuafabPath, false);
                await craftLuafabLoading.WaitTask;
                var slotRoot = (SlotBehaviour)craftLuafabLoading.RawFork(transform);
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

                var craftJson = miniManager.args.craftJson;
                var altasTex = miniManager.args.atlasTex;
                if (craftJson != null)
                {
                    var unpackContext = new CraftUnpackContext(craftJson, altasTex);
                    unpackContext.UnpackRoot(slotRoot);
                }
                craftSlot = slotRoot;
            }
        }
        public async UniTask EditMain()
        {
            editCamera.gameObject.SetActive(true);
            var craftLuafabLoading = miniManager.assetModule.AttachLuafabLoading(miniManager.bridge.envPaths.miniCraftLuafabPath, false);
            await craftLuafabLoading.WaitTask;
            craftSlot = (SlotBehaviour)craftLuafabLoading.RawFork(transform);
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
