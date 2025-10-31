using System.IO;
using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using XLua;

namespace Nianxie.Preview
{
    public class PreviewBridge: MiniBridge
    {
        public PreviewBridge(PreviewGizmos previewGizmos, AssetBundle bundle) : base(EditorGetMiniBoot(), bundle.CheckMiniFolder(), bundle)
        {
            gizmos = previewGizmos;
        }
        public PreviewBridge(PreviewGizmos previewGizmos, string folder) : base(EditorGetMiniBoot(), folder, null)
        {
            gizmos = previewGizmos;
        }

        private PreviewGizmos gizmos;
        private LuaEnv luaEnv;
        private LuaFunction bridgeWrapFn;
        private MiniGameManager miniManager;
        public CraftEdit craftEdit { get; private set; }

        public async UniTask Main(bool editCraft)
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
            var selfWrap = bridgeWrapFn.Func<PreviewBridge, LuaTable>(this);
            miniManager = await LoadMini();
            if (editCraft)
            {
                var args = new MiniEditArgs
                {
                    refresh=selfWrap.Get<LuaFunction>(nameof(GizmosRefresh)),
                };
                craftEdit = await miniManager.EditMain(args);
            }
            else
            {
                var args = new MiniPlayArgs
                {
                    playEnding=selfWrap.Get<LuaFunction>(nameof(ExecuteEnding)),
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
        }

        public void Unload()
        {
            gizmos.Refresh(null);
            if (bundle != null)
            {
                bundle.UnloadAsync(true);
            }
            if (miniManager != null)
            {
                UnityEngine.Object.Destroy(miniManager);
                miniManager = null;
            }
        }

        public void ExecuteEnding()
        {
            Debug.Log("假装播放一下结束视频");
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

        public void GizmosRefresh()
        {
            gizmos.Refresh(craftEdit);
        }
        private static byte[] EditorGetMiniBoot()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(NianxieConst.MiniBootPath).bytes;
#else
            return null;
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