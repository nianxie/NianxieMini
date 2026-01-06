using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Riff;
using Nianxie.Utils;
using UnityEngine;
using System.IO;
using WebP;

namespace Nianxie.Preview
{
    public class PreviewBridge:MiniBridge
    {
        private static byte[] miniBootBytes => PreviewAssets.instance.miniBoot.bytes;
        public Texture2D webpTexture { get; private set; }

        public PreviewBridge(string folder) : base(miniBootBytes, folder, null, null)
        {
        }
        public PreviewBridge(byte[] miniBoot, AssetBundle bundle) : base(miniBoot, bundle.CheckMiniFolder(), bundle, null)
        {
        }
        public PreviewBridge(AssetBundle bundle) : base(miniBootBytes, bundle.CheckMiniFolder(), bundle, null)
        {
        }

        public async UniTask OpenCraft()
        {
            var configTextAsset = await LoadAssetAsync<TextAsset>(envPaths.miniProjectConfig);
            miniConfig = MiniProjectConfig.FromJson(configTextAsset.bytes);
            if (miniConfig.craftable)
            {
                var (riffBytes, tex) = OpenPanelLoadCraftFiles();
                if (riffBytes != null)
                {
                    webpTexture = tex;
                    riffPackage = await RiffPackage.Create(riffBytes, tex);
                }
            }
        }

        public void Unload()
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
            }

            if (riffPackage != null)
            {
                UnityEngine.Object.Destroy(riffPackage);
                riffPackage = null;
            }

            if (webpTexture != null)
            {
                UnityEngine.Object.Destroy(webpTexture);
                webpTexture = null;
            }
        }

#if UNITY_EDITOR
        
        public override async UniTask<UnityEngine.Object> LoadAssetAsync(string resPath, System.Type resType)
        {
            if (assetBundle != null)
            {
                return await base.LoadAssetAsync(resPath, resType);
            }
            else
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath(resPath, resType);
            }
        }

        public override async UniTask<UnityEngine.Object[]> LoadSubAssetsAsync(string resPath)
        {
            if (assetBundle != null)
            {
                return await base.LoadSubAssetsAsync(resPath);
            }
            else
            {
                return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(resPath);
            }
        }
#endif
        private static (byte[], Texture2D) OpenPanelLoadCraftFiles()
        {
#if UNITY_EDITOR
            var selectPath = UnityEditor.EditorUtility.OpenFilePanel("Open Craft Game", Directory.GetCurrentDirectory(), NianxieConst.Ext.CRAFT);
            if (!string.IsNullOrEmpty(selectPath))
            {
                var craftPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.{NianxieConst.Ext.CRAFT}";
                var riffBytes = File.ReadAllBytes(craftPath);
                var atlasTex = Texture2DExt.CreateTexture2DFromWebP(riffBytes, false, false, out var err);
                if (err != Error.Success)
                {
                    throw new System.Exception($"webp load error {err.ToString()}");
                }
                return (riffBytes, atlasTex);
            }
            return (null, null);
#else
            throw new System.NotImplementedException();
#endif
        }
    }
}