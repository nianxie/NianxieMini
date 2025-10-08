using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Components;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;

namespace Nianxie.Craft
{
    public class EditCraftModule : AbstractGameModule
    {
        public Camera camera;
        public SlotBehaviour craftSlot { get; private set; }

        public async UniTask Main(MiniGameManager miniManager)
        {
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
