using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
using XLua.LuaDLL;

namespace Nianxie.Framework
{
    public struct MiniPlayArgs
    {
        public LuaFunction playEnding;
        public CraftJson craftJson;
        public Texture2D atlasTex;

        public void PlayEnding(MiniGameManager miniManager)
        {
            playEnding.Action(miniManager);
        }
    }
    public struct MiniEditArgs
    {
        public LuaFunction refresh;
    }

    public class MiniBridge: IAssetLoader
    {
        protected readonly AssetBundle bundle;
        public readonly EnvPaths envPaths;
        public readonly byte[] miniBoot;
        public MiniProjectConfig miniConfig { get; private set; }
        public MiniBridge(byte[] miniBoot, string folder, AssetBundle bundle)
        {
            this.bundle = bundle;
            this.miniBoot = miniBoot;
            envPaths = EnvPaths.MiniEnvPaths(folder);
        }

        public async UniTask<MiniGameManager> LoadMini()
        {
            var scene = await SceneAsyncUtility.LoadSceneAsync(NianxieConst.MiniSceneName);
            var objList = scene.GetRootGameObjects();
            var miniManager = objList[0].GetComponent<MiniGameManager>();
            SceneManager.SetActiveScene(scene);
            Assert.IsTrue(objList.Length == 1, "mini scene's root object is not one and only one");
            await miniManager.PreInit(this);
            // TODO, 这里需要注意一下加载的时序问题，可能在加载中，玩家返回了。
            return miniManager;
        }

        #region // 以下是AssetLoader的相关函数
        public async UniTask<Dictionary<string, TextAsset>> LoadScriptAssetsAsync()
        {
            var configTextAsset = await LoadAssetAsync<TextAsset>(envPaths.miniProjectConfig);
            miniConfig = MiniProjectConfig.FromJson(configTextAsset.bytes);
            var retScriptDict = new Dictionary<string, TextAsset>();
            // 预加载 lua text asset
            UniTask[] preloadTask = new UniTask[miniConfig.scripts.Length];
            for (int i = 0; i < miniConfig.scripts.Length; i++)
            {
                var path = $"{envPaths.pathPrefix}/{miniConfig.scripts[i]}";
                preloadTask[i] = UniTask.Create(async () =>
                {
                    retScriptDict[path] = await LoadAssetAsync<TextAsset>(path);
                });
            }
            await UniTask.WhenAll(preloadTask);
            return retScriptDict;
        }
        public async UniTask<TObject> LoadAssetAsync<TObject>(string resPath) where TObject: UnityEngine.Object
        {
            var obj = await LoadAssetAsync(resPath, typeof(TObject));
            return (TObject) obj;
        }
        

        public virtual async UniTask<UnityEngine.Object> LoadAssetAsync(string resPath, Type resType)
        {
            return await bundle.LoadAssetAsync(resPath, resType).ToUniTask();
        }

        public virtual async UniTask<UnityEngine.Object[]> LoadSubAssetsAsync(string resPath)
        {
            var request = bundle.LoadAssetWithSubAssetsAsync(resPath);
            await request.ToUniTask();
            return request.allAssets;
        }
        #endregion
    }
}