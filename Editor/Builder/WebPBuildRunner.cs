using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Nianxie.Editor
{
    public class WebPBuildRunner:AbstractBuildRunner
    {
        private AssetBundleBuild webpBundle;
        private int quality;
        public WebPBuildRunner(MiniEditorEnvPaths envPaths, BuildTarget[] buildTargets, int quality) : base(envPaths, buildTargets)
        {
            this.quality = quality;
            webpBundle = new AssetBundleBuild()
            {
                assetBundleName = $"{envPaths.miniId}_webp",
                assetBundleVariant = "",
                assetNames = null,
            };
        }
        
        protected override string serializeKind => MiniEditorEnvPaths.WBUNDLE_EXT;

        protected override async void PrePostAndBuild(Action<BundleInfo[]> onSucc)
        {
            var notScriptDict = CollectNotScript.Collect(envPaths.reflectEnv);

            // main bundle，包含script和非tex资源
            var explicitCollects = notScriptDict.Values.Where(a => a.isExplicit && !a.canWebP);
            mainBundle.assetNames = envPaths.reflectEnv.scriptAssetDict.Keys
                .Concat(new []{envPaths.miniProjectConfig})
                .Concat(explicitCollects.Select(a=>a.path)).ToArray();
            
            // webp bundle，包含webp tex资源
            var webpCollects = notScriptDict.Values.Where(a => a.canWebP).ToArray();
            webpBundle.assetNames = webpCollects.Select(a => a.path).ToArray();

            foreach (var webpCollect in webpCollects)
            {
                webpCollect.OverrideSettingsWithRGBA32();
            }
            PipelineBuild(new []{mainBundle, webpBundle});
            foreach (var webpCollect in webpCollects)
            {
                webpCollect.RollbackSettings();
            }
            EditorUtility.DisplayProgressBar("merge bundle", "merge bundle", 0.5f);
            try
            {
                foreach (var buildTarget in buildTargets)
                {
                    string platformOutputDir = envPaths.GetPlatformBuildDir(buildTarget);
                    string bundlePathNoExt = $"{platformOutputDir}/{webpBundle.assetBundleName}";
                    string uncompressPath = $"{bundlePathNoExt}_uncompress";
                    var oper = AssetBundle.RecompressAssetBundleAsync($"{bundlePathNoExt}", uncompressPath,
                        BuildCompression.Uncompressed);
                    while (!oper.isDone)
                    {
                        await Task.Delay(100);
                    }

                    byte[] mainBundleData = await File.ReadAllBytesAsync($"{platformOutputDir}/{mainBundle.assetBundleName}");
                    /*var textureBundle = new AssetStudio.TextureBundle(uncompressPath);
                    textureBundle.DumpTwoFileAndWebpRecode($"{envPaths.buildDir}/{envPaths.miniId}_{buildTarget}.{MiniEditorEnvPaths.WBUNDLE_EXT}", mainBundleData, quality);*/
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Build failed when pack bundle");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            onSucc(null);
        }
    }
}