using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

namespace Nianxie.Preview
{
    public abstract class PreviewGame: MiniBridge
    {
        private static byte[] miniBootBytes => PreviewAssets.instance.miniBoot.bytes;
        private PreviewGame(byte[] miniBoot, string folder, AssetBundle bundle) : base(miniBoot, folder, bundle)
        {
        }
        public class EditGame:PreviewGame
        {
            private PreviewEditView editView;
            public EditGame(AssetBundle bundle) : base(miniBootBytes, bundle.CheckMiniFolder(), bundle)
            {
            }
            public EditGame(string folder) : base(miniBootBytes, folder, null)
            {
            }
            public async UniTask Main(Func<Transform, PreviewEditView> makeEditView, bool useCraftFile, Action<bool> reopen)
            {
                var selfWrap = await InitFakeShell();
                var args = new MiniEditArgs
                {
                    shellRefresh=selfWrap.Get<LuaFunction>(nameof(GizmosRefresh)),
                    shellRelease=selfWrap.Get<LuaFunction>(nameof(GizmosRelease)),
                };
                var craftEdit = await miniManager.EditMain(args);
                editView = makeEditView(craftEdit.editCanvas.transform);
                editView.Main(craftEdit, reopen);
            }
            public void GizmosRefresh()
            {
                if (editView == null) return;
                editView.gizmos.Refresh();
            }
            public void GizmosRelease(UnityEngine.Object resObj)
            {
                if (editView == null) return;
                editView.gizmos.Release(resObj);
            }
        }
        public class PlayGame:PreviewGame
        {
            private Action<string> playEnding;
            public PlayGame(byte[] miniBoot, AssetBundle bundle) : base(miniBoot, bundle.CheckMiniFolder(), bundle)
            {
            }
            public PlayGame(AssetBundle bundle) : base(miniBootBytes, bundle.CheckMiniFolder(), bundle)
            {
            }
            public PlayGame(string folder) : base(miniBootBytes, folder, null)
            {
            }

            public async UniTask Main(Action<string> playEnding)
            {
                this.playEnding = playEnding;
                var selfWrap = await InitFakeShell();
                var args = new MiniPlayArgs
                {
                    playEnding=selfWrap.Get<LuaFunction>(nameof(PlayEnding)),
                };
                if (miniConfig.craftable)
                {
                    var (craftJson, atlasTex) = OpenPanel();
                    args.craftJson = craftJson;
                    args.atlasTex = atlasTex;
                }
                else
                {
                    await miniManager.PlayMain(args);
                }
        
            }
            public void PlayEnding()
            {
                playEnding(miniConfig.previewVideoUrl);
            }
        }

        private LuaEnv luaEnv;
        private LuaFunction bridgeWrapFn;
        private MiniGameManager miniManager;

        private async UniTask<LuaTable> InitFakeShell()
        {
            luaEnv = new LuaEnv();
            bridgeWrapFn = luaEnv.LoadString<LuaFunction>(@"
local bridge = ...
return setmetatable({
}, {
    __index=function(t,k)
        return function(...)
            return bridge[k](bridge, ...)
        end
    end
})
");
            var selfWrap = bridgeWrapFn.Func<PreviewGame, LuaTable>(this);
            miniManager = await LoadMini();
            return selfWrap;
        }

        public void Unload()
        {
            if (bundle != null)
            {
                bundle.Unload(true);
            }
            if (miniManager != null)
            {
                UnityEngine.Object.Destroy(miniManager);
                miniManager = null;
            }
        }

        private (CraftJson, Texture2D) OpenPanel()
        {
#if UNITY_EDITOR
            var selectPath = UnityEditor.EditorUtility.OpenFilePanel("Open Craft Game", Path.Combine(Application.dataPath, ".."), "json,png");
            if (!string.IsNullOrEmpty(selectPath))
            {
                var jsonPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.json";
                var pngPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.png";
                var craftJson = CraftJson.FromLargeBytes(new LargeBytes(File.ReadAllBytes(jsonPath)));
                var atlasTex = new Texture2D(1, 1);
                atlasTex.LoadImage(File.ReadAllBytes(pngPath));
                return (craftJson, atlasTex);
            }
            return (null, null);
#else
            throw new System.NotImplementedException();
#endif
        }
#if UNITY_EDITOR
        
        public override async UniTask<Object> LoadAssetAsync(string resPath, System.Type resType)
        {
            if (bundle == null)
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath(resPath, resType);
            }
            else
            {
                return await bundle.LoadAssetAsync(resPath, resType).ToUniTask();
            }
        }

        public override async UniTask<Object[]> LoadSubAssetsAsync(string resPath)
        {
            if (bundle == null)
            {
                return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(resPath);
            }
            else
            {
                var request = bundle.LoadAssetWithSubAssetsAsync(resPath);
                await request.ToUniTask();
                return request.allAssets;
            }
        }
#endif
    }
}