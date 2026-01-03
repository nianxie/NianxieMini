using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using WebP;
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

        public class EditReopenArgs
        {
            public PreviewEditView.ReopenKind kind;
            public CraftJson craftJson;
            public Texture2D atlasTex;
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
            public async UniTask Main(Func<Transform, PreviewEditView> makeEditView, EditReopenArgs reopenArgs, Action<EditReopenArgs> reopen)
            {
                var selfWrap = await InitFakeShell();
                if (reopenArgs!=null)
                {
                    if (reopenArgs.kind == PreviewEditView.ReopenKind.LOAD)
                    {
                        (craftJson, atlasTex) = OpenPanelLoadCraftFiles();
                    }
                    else
                    {
                        craftJson = reopenArgs.craftJson;
                        atlasTex = reopenArgs.atlasTex;
                    }
                }
                var args = new MiniEditArgs
                {
                    shellRefresh=selfWrap.Get<LuaFunction>(nameof(GizmosRefresh)),
                    shellRelease=selfWrap.Get<LuaFunction>(nameof(GizmosRelease)),
                    //craftJson=craftJson,
                    atlasTex=atlasTex,
                };
                await miniManager.entry.EditMain(args);
                var craftEdit = (miniManager.entry as CraftEntryModule)!.craftEdit;
                editView = makeEditView(craftEdit.editCanvas.transform);
                editView.Main(craftEdit, (reopenKind) =>
                {
                    if (reopenKind == PreviewEditView.ReopenKind.RESET)
                    {
                        var reserveTex = atlasTex;
                        atlasTex = null;
                        reopen(new EditReopenArgs()
                        {
                            kind = reopenKind,
                            craftJson = craftJson,
                            atlasTex = reserveTex,
                        });
                    }
                    else
                    {
                        reopen(new EditReopenArgs()
                        {
                            kind = reopenKind,
                        });
                    }
                });
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
                    var (craftJson, atlasTex) = OpenPanelLoadCraftFiles();
                    //args.craftJson = craftJson;
                    args.atlasTex = atlasTex;
                }
                else
                {
                    await miniManager.entry.PlayMain(args);
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
        private CraftJson craftJson;
        private Texture2D atlasTex;

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

            if (atlasTex != null)
            {
                UnityEngine.Object.Destroy(atlasTex);
                atlasTex = null;
            }

            if (miniManager != null)
            {
                UnityEngine.Object.Destroy(miniManager);
                miniManager = null;
            }
        }

        private static (CraftJson, Texture2D) OpenPanelLoadCraftFiles()
        {
#if UNITY_EDITOR
            var selectPath = UnityEditor.EditorUtility.OpenFilePanel("Open Craft Game", Path.Combine(Application.dataPath, ".."), "json,png");
            if (!string.IsNullOrEmpty(selectPath))
            {
                var jsonPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.json";
                var webpPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.webp";
                var craftJson = CraftJson.FromLargeBytes(new LargeBytes(File.ReadAllBytes(jsonPath)));
                var atlasTex = Texture2DExt.CreateTexture2DFromWebP(File.ReadAllBytes(webpPath), false, false, out var err);
                if (err != Error.Success)
                {
                    throw new Exception($"webp load error {err.ToString()}");
                }

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