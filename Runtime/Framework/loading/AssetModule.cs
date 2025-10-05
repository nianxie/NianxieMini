using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using UnityEngine;

namespace Nianxie.Framework
{
    public interface ICacheLoader
    {
        public LuafabLoading CacheLuafabLoading(string prefabPath, bool lazy);
        public AssetLoading CacheAssetLoading(string assetPath, System.Type assetType);
        public SubAssetsLoading CacheSubAssetsLoading(string subAssetsPath);
        public AbstractGameManager GetGameManager();
    }

    public interface IAssetLoader
    {
        public UniTask<Dictionary<string, TextAsset>> LoadScriptAssetsAsync();
        public UniTask<Object> LoadAssetAsync(string resPath, System.Type resType);
        public UniTask<Object[]> LoadSubAssetsAsync(string resPath);
    }

    public class AssetModule : AbstractGameModule, ICacheLoader
    {
        private readonly Dictionary<string, TextAsset> scriptAssetDict = new ();
        private readonly Dictionary<string, LuafabLoading> luafabLoadingDict = new ();
        private readonly Dictionary<string, Dictionary<System.Type, AssetLoading>> assetLoadingDictDict = new ();
        private readonly Dictionary<string, SubAssetsLoading> subAssetsLoadingDict = new ();
        private IAssetLoader assetLoader;
        public void PreInit(IAssetLoader assetLoader)
        {
            this.assetLoader = assetLoader;
        }
        public override async UniTask Init()
        {
            var dict = await assetLoader.LoadScriptAssetsAsync();
            foreach (var kv in dict)
            {
                scriptAssetDict[kv.Key] = kv.Value;
            }
        }
        LuafabLoading ICacheLoader.CacheLuafabLoading(string prefabPath, bool lazy)
        {
            if(luafabLoadingDict.TryGetValue(prefabPath, out var loading))
            {
                if (!lazy)
                {
                    loading.Start();
                }
                return loading;
            }
            var prefabLoading = new LuafabLoading(prefabPath, this);
            luafabLoadingDict[prefabPath] = prefabLoading;
            if (!lazy)
            {
                prefabLoading.Start();
            }
            return prefabLoading;
        }

        AssetLoading ICacheLoader.CacheAssetLoading(string assetPath, System.Type assetType)
        {
            if(!assetLoadingDictDict.TryGetValue(assetPath, out var assetLoadingDict))
            {
                assetLoadingDict = new();
                assetLoadingDictDict[assetPath] = assetLoadingDict;
            }

            if (assetLoadingDict.TryGetValue(assetType, out var loading))
            {
                return loading;
            }

            var assetLoading = new AssetLoading(assetPath, assetType, assetLoader);
            assetLoadingDict[assetType] = assetLoading;
            assetLoading.Start();
            return assetLoading;
        }

        SubAssetsLoading ICacheLoader.CacheSubAssetsLoading(string subAssetsPath)
        {
            if(subAssetsLoadingDict.TryGetValue(subAssetsPath, out var loading))
            {
                return loading;
            }
            var subAssetsLoading = new SubAssetsLoading(subAssetsPath, assetLoader);
            subAssetsLoadingDict[subAssetsPath] = subAssetsLoading;
            subAssetsLoading.Start();
            return subAssetsLoading;
        }

        AbstractGameManager ICacheLoader.GetGameManager()
        {
            return gameManager;
        }
        
        public IReadOnlyDictionary<string, TextAsset> GetScriptAssetDict()
        {
            return scriptAssetDict;
        }

        public LuafabLoading AttachLuafabLoading(string fullPath, bool lazy)
        {
            return ((ICacheLoader) this).CacheLuafabLoading(fullPath, lazy);
        }

        public Object GetTypedAsset(string resPath, System.Type resType)
        {
            return assetLoadingDictDict[resPath][resType].asset;
        }
        public Object GetSubAsset(string path, string name)
        {
            return subAssetsLoadingDict[path].GetSubAsset(name);
        }
    }
}
