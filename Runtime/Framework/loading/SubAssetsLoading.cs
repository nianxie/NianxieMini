
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Nianxie.Framework
{
    public class SubAssetsLoading: AbstractLoading<UnityEngine.Object[]>
    {
        private readonly Dictionary<string, UnityEngine.Object> subAssetDict = new();
        private IAssetLoader assetLoader;
        public SubAssetsLoading(
            string resPath, 
            IAssetLoader assetLoader
        ) : base(resPath)
        {
            this.assetLoader = assetLoader;
        }

        public UnityEngine.Object GetSubAsset(string name)
        {
            return subAssetDict[name];
        }

        protected override async UniTask<UnityEngine.Object[]> LoadAsync()
        {
            var objs = await assetLoader.LoadSubAssetsAsync(resPath);
            foreach (var asset in objs)
            {
                subAssetDict[asset.name] = asset;
            }
            return objs;
        }
    }
}