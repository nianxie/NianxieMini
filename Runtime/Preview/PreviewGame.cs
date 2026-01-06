using System;
using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Framework;
using Nianxie.Riff;
using Nianxie.Utils;
using UnityEngine;
using XLua;

namespace Nianxie.Preview
{
    public abstract class PreviewGame
    {
        public class EditGame:PreviewGame
        {
            private PreviewEditView editView;

            public async UniTask InitBridgeByPath(string folder, string bundlePath)
            {
                PreviewBridge previewBridge;
                if (string.IsNullOrEmpty(bundlePath))
                {
                    previewBridge = new PreviewBridge(folder);
                }
                else
                {
                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    previewBridge = new PreviewBridge(bundle);
                }
                bridge = previewBridge;
            }
            public async UniTask Main(Func<Transform, PreviewEditView> makeEditView, Action<EditGame> onReopen)
            {
                var selfWrap = InitFakeShell();
                var args = new MiniEditArgs
                {
                    shellRefresh=selfWrap.Get<LuaFunction>(nameof(GizmosRefresh)),
                    shellRelease=selfWrap.Get<LuaFunction>(nameof(GizmosRelease)),
                    //craftJson=craftJson,
                    //atlasTex=atlasTex,
                };
                miniManager = await SceneAsyncUtility.CreateMiniGameAsync(bridge);
                await miniManager.entry.EditMain(args);
                var craftEdit = (miniManager.entry as CraftEntryModule)!.craftEdit;
                editView = makeEditView(craftEdit.editCanvas.transform);
                editView.Main(craftEdit, (kind) =>
                {
                    var reserveBridge = bridge;
                    bridge = null;
                    Unload();
                    var newEditGame = new EditGame();
                    newEditGame.bridge = bridge;
                    onReopen(newEditGame);
                    if (kind == PreviewEditView.ReopenKind.RESET)
                    {
                    }
                    else
                    {
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

            public void InitBridgeByRaw(PreviewBridge bridge)
            {
                this.bridge = bridge;
            }
            public async UniTask InitBridgeByPath(string folder, string bundlePath)
            {
                PreviewBridge previewBridge;
                if (string.IsNullOrEmpty(bundlePath))
                {
                    previewBridge = new PreviewBridge(folder);
                }
                else
                {
                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    previewBridge = new PreviewBridge(bundle);
                }

                bridge = previewBridge;
                await previewBridge.OpenCraft();
            }
            public async UniTask Main(Action<string> playEnding)
            {
                this.playEnding = playEnding;
                var selfWrap = InitFakeShell();
                var args = new MiniPlayArgs
                {
                    playEnding=selfWrap.Get<LuaFunction>(nameof(PlayEnding)),
                };
                miniManager = await SceneAsyncUtility.CreateMiniGameAsync(bridge);
                await miniManager.entry.PlayMain(args);
        
            }
            public void PlayEnding()
            {
                playEnding(bridge.miniConfig.previewVideoUrl);
            }
        }
        
        public class EditReopenArgs
        {
            public PreviewEditView.ReopenKind kind;
            public CraftJson craftJson;
            public Texture2D atlasTex;
        }

        private MiniGameManager miniManager;
        private LuaEnv luaEnv;
        
        private PreviewBridge bridge;

        private LuaTable InitFakeShell()
        {
            luaEnv = new LuaEnv();
            var wrapFn = luaEnv.LoadString<LuaFunction>(@"
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
            var selfWrap = wrapFn.Func<PreviewGame, LuaTable>(this);
            return selfWrap;
        }


        public void Unload()
        {
            if (bridge != null)
            {
                bridge.Unload();
                bridge = null;
            }

            if (miniManager != null)
            {
                UnityEngine.Object.Destroy(miniManager);
                miniManager = null;
            }

            if (luaEnv != null)
            {
                luaEnv.Dispose();
                luaEnv = null;
            }
        }
    }
}