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
    public class CraftModule : AbstractGameModule
    {
        public EditRoot editRoot;
        public SlotBehaviour craftSlot { get; private set; }
        private MiniGameManager miniManager => (MiniGameManager) gameManager;
        public MiniEditArgs editArgs { get; private set; }
        public MiniPlayArgs playArgs { get; private set; }

        public async UniTask PlayMain(MiniPlayArgs args)
        {
            playArgs = args;
            editRoot.camera.gameObject.SetActive(false);
            if (miniManager.bridge.miniConfig.craftable)
            {
                var craftLuafabLoading =
                    miniManager.assetModule.AttachLuafabLoading(miniManager.bridge.envPaths.miniCraftLuafabPath, false);
                await craftLuafabLoading.WaitTask;
                var slotRoot = (SlotBehaviour) craftLuafabLoading.RawFork(editRoot.area.transform);
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
            editRoot.camera.gameObject.SetActive(true);
            var craftLuafabLoading =
                miniManager.assetModule.AttachLuafabLoading(miniManager.bridge.envPaths.miniCraftLuafabPath, false);
            await craftLuafabLoading.WaitTask;
            craftSlot = (SlotBehaviour) craftLuafabLoading.RawFork(editRoot.area.transform);
            editRoot.Init(this, craftSlot);
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
