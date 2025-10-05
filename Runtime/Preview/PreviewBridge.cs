using System.IO;
using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nianxie.Preview
{
    public class PreviewBridge: MiniBridge
    {
        public MiniGameManager miniManager;
        private EnvPaths envPaths;
        private Scene scene;
        private AssetBundle assetBundle;

        public void Main(PreviewManager previewManager, string miniId, string bundlePath)
        {
            envPaths = EnvPaths.MiniEnvPaths(miniId);
            UniTask.Create(async () =>
            {
                if (!string.IsNullOrEmpty(bundlePath))
                {
                    assetBundle = await AssetBundle.LoadFromFileAsync(bundlePath);
                }

                (scene, miniManager) = await previewManager.LoadScene();
                SceneManager.SetActiveScene(scene);
                var session = new MiniBridgeSession(scene, (_) =>
                {
                    ExecuteEnding();
                });
                if (previewManager.editCraft)
                {
                    await miniManager.EditCraftMain(this, session);
                }
                else
                {
                    var configTextAsset = await LoadAssetAsync<TextAsset>(envPaths.miniProjectConfig);
                    var config = MiniProjectConfig.FromJson(configTextAsset.bytes);
                    if (config.craft)
                    {
                        var (craftJson, atlasTex) = OpenPanel();
                        await miniManager.PlayCraftMain(this, session, craftJson, atlasTex);
                    }
                    else
                    {
                        await miniManager.PlayGameMain(this, session);
                    }
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

        public override EnvPaths GetEnvPaths()
        {
            return envPaths;
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
                var craftJson = CraftJson.Load(File.ReadAllBytes(jsonPath));
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