using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Nianxie.Framework
{
    
    public class AssetLoading: AbstractLoading<UnityEngine.Object>
    {
        public UnityEngine.Object asset { get; private set; }
        private System.Type resType;
        private IAssetLoader assetLoader;

        public AssetLoading(
            string resPath, 
            System.Type resType, 
            IAssetLoader assetLoader
        ) : base(resPath)
        {
            this.resType = resType;
            this.assetLoader = assetLoader;
        }

        protected override async UniTask<UnityEngine.Object> LoadAsync()
        {
            var obj= await assetLoader.LoadAssetAsync(resPath, resType);
            asset = obj;
            return obj;
        }
    }
}