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
        public MiniGameManager miniManager;
        private Scene scene;
        private AssetBundle assetBundle;

        public void Main(PreviewManager previewManager, LuaTable selfWrap, string folder, string bundlePath)
        {
            // enable touch simulation
            // TouchSimulation.Enable();
            envPaths = EnvPaths.MiniEnvPaths(folder);
            UniTask.Create(async () =>
            {
                if (!string.IsNullOrEmpty(bundlePath))
                {
                    assetBundle = await AssetBundle.LoadFromFileAsync(bundlePath);
                }
                var configTextAsset = await LoadAssetAsync<TextAsset>(envPaths.miniProjectConfig);
                var config = MiniProjectConfig.FromJson(configTextAsset.bytes);

                (scene, miniManager) = await previewManager.LoadScene();
                SceneManager.SetActiveScene(scene);
                await miniManager.PreInit(this);
                if (previewManager.editCraft)
                {
                    var args = new MiniEditArgs
                    {
                        onSelect=selfWrap.Get<LuaFunction>(nameof(OnSelect)),
                    };
                    await miniManager.EditMain(args);
                }
                else
                {
                    var args = new MiniPlayArgs
                    {
                        playEnding=null,
                        craft=config.craft,
                    };
                    if (config.craft)
                    {
                        var (craftJson, atlasTex) = OpenPanel();
                        args.craftJson = craftJson;
                        args.atlasTex = atlasTex;
                    }
                    await miniManager.PlayMain(args);
                }
            }).Forget();
        }

        public void OnDestroy()
        {
            if (assetBundle != null)
            {
                assetBundle.UnloadAsync(true);
                assetBundle = null;
            }
            SceneManager.UnloadSceneAsync(scene);
        }
        
        public override async UniTask UnloadMini(MiniGameManager miniManager)
        {
            Destroy(this);
        }

        private void ExecuteEnding()
        {
            Debug.Log("假装播放一下结束视频");
        }
        
        public (CraftJson, Texture2D) OpenPanel()
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
        public void OnSelect(AbstractAssetSlot slot)
        {
#if UNITY_EDITOR
            if (slot != null)
            {
                Debug.Log($"try select {slot.gameObject.name}");
                UnityEditor.Selection.activeGameObject = slot.gameObject;
            }
            else
            {
                UnityEditor.Selection.activeGameObject = null;
            }
#endif
        }
#if UNITY_EDITOR
        public override byte[] GetMiniBoot()
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(NianxieConst.MiniBootPath).bytes;
        }
        
        public override async UniTask<Object> LoadAssetAsync(string resPath, System.Type resType)
        {
            if (assetBundle == null)
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath(resPath, resType);
            }
            else
            {
                return await assetBundle.LoadAssetAsync(resPath, resType).ToUniTask();
            }
        }

        public override async UniTask<Object[]> LoadSubAssetsAsync(string resPath)
        {
            if (assetBundle == null)
            {
                return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(resPath);
            }
            else
            {
                var request = assetBundle.LoadAssetWithSubAssetsAsync(resPath);
                await request.ToUniTask();
                return request.allAssets;
            }
        }

#endif
    }
}