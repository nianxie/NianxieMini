using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Nianxie.Framework
{
    public class MiniBridgeSession
    {
        public Scene scene { get; }
        private System.Action<MiniBridgeSession> ending { get; }

        public MiniBridgeSession(Scene scene, System.Action<MiniBridgeSession> ending)
        {
            this.scene = scene;
            this.ending = ending;
        }

        public void PlayEnding()
        {
            ending(this);
        }
    }

    public abstract class MiniBridge:MonoBehaviour, IAssetLoader
    {
        public abstract EnvPaths GetEnvPaths();

        public virtual byte[] GetMiniBoot()
        {
            throw new System.NotImplementedException();
        }

        #region // 以下是AssetLoader的相关函数
        public async UniTask<Dictionary<string, TextAsset>> LoadScriptAssetsAsync()
        {
            var envPaths = GetEnvPaths();
            var configTextAsset = await LoadAssetAsync<TextAsset>(envPaths.miniProjectConfig);
            var config = MiniProjectConfig.FromJson(configTextAsset.bytes);
            var retScriptDict = new Dictionary<string, TextAsset>();
            // 预加载 lua text asset
            UniTask[] preloadTask = new UniTask[config.scripts.Length];
            for (int i = 0; i < config.scripts.Length; i++)
            {
                var path = $"{envPaths.pathPrefix}/{config.scripts[i]}";
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
        

        public virtual UniTask<UnityEngine.Object> LoadAssetAsync(string resPath, Type resType)
        {
            throw new NotImplementedException();
        }

        public virtual UniTask<UnityEngine.Object[]> LoadSubAssetsAsync(string resPath)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}