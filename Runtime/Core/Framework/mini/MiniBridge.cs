using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Riff;
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

        public void PlayEnding(MiniGameManager miniManager)
        {
            playEnding.Action(miniManager);
        }
    }

    public class MiniBridge: IAssetLoader
    {
        protected AssetBundle assetBundle { get; }
        public RiffPackage riffPackage { get; protected set; }
        public readonly EnvPaths envPaths;
        public readonly byte[] miniBoot;
        public MiniProjectConfig miniConfig { get; protected set; }
        public MiniBridge(byte[] miniBoot, string folder, AssetBundle assetBundle, RiffPackage riffPackage)
        {
            this.assetBundle = assetBundle;
            this.miniBoot = miniBoot;
            this.riffPackage = riffPackage;
            envPaths = EnvPaths.RuntimeEnvPaths(EnvPaths.miniFolder2pathPrefix(folder));
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
        protected async UniTask<TObject> LoadAssetAsync<TObject>(string resPath) where TObject: UnityEngine.Object
        {
            var obj = await LoadAssetAsync(resPath, typeof(TObject));
            return (TObject) obj;
        }
        

        public virtual async UniTask<UnityEngine.Object> LoadAssetAsync(string resPath, Type resType)
        {
            return await assetBundle.LoadAssetAsync(resPath, resType).ToUniTask();
        }

        public virtual async UniTask<UnityEngine.Object[]> LoadSubAssetsAsync(string resPath)
        {
            var request = assetBundle.LoadAssetWithSubAssetsAsync(resPath);
            await request.ToUniTask();
            return request.allAssets;
        }
        #endregion
    }
}