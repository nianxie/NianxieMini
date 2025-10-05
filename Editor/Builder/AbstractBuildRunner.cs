using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor;
using UnityEngine;

namespace Nianxie.Editor
{
    public abstract class AbstractBuildRunner
    {
        protected AssetBundleBuild mainBundle;
        protected BuildTarget[] buildTargets;
        protected MiniEditorEnvPaths envPaths;

        protected AbstractBuildRunner(MiniEditorEnvPaths envPaths, BuildTarget[] buildTargets)
        {
            this.envPaths = envPaths;
            this.buildTargets = buildTargets;
            mainBundle = new AssetBundleBuild()
            {
                assetBundleName = envPaths.miniId,
                assetBundleVariant = "",
                assetNames = null,
            };
        }

        protected abstract string serializeKind { get; }
        protected abstract void PrePostAndBuild(Action<BundleInfo[]> onSucc);

        protected void PipelineBuild(AssetBundleBuild[] bundleBuilds)
        {
            foreach (var buildTarget in buildTargets)
            {
                string platformOutputDirectory = envPaths.GetPlatformBuildDir(buildTarget);
                var buildOptions = GetBundleBuildOptions();
                AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(platformOutputDirectory, bundleBuilds, buildOptions, buildTarget);
                if (unityManifest == null)
                {
                    string message = "UnityEngine build failed !";
                    throw new Exception(message);
                }

                // 检测输出目录
                string unityOutputManifestFilePath = $"{platformOutputDirectory}/{Path.GetFileName(platformOutputDirectory)}";
                if (!File.Exists(unityOutputManifestFilePath))
                {
                    string message = $"Not found output {nameof(AssetBundleManifest)} file : {unityOutputManifestFilePath}";
                    throw new Exception(message);
                }
                Debug.Log($"UnityEngine build success in platform {buildTarget}!");
            }
        }
        
        /// <summary>
        /// 获取内置构建管线的构建选项
        /// </summary>
        private BuildAssetBundleOptions GetBundleBuildOptions()
        {
            // For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
            // 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

            opt |= BuildAssetBundleOptions.ChunkBasedCompression;

            // opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles

            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName; //Disables Asset Bundle LoadAsset by file name.
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension; //Disables Asset Bundle LoadAsset by file name with extension.			

            return opt;
        }
        
        protected string GetPlatformOutputDirectory(BuildTarget buildTarget)
        {
            return $"{envPaths.buildDir}/{buildTarget}";
        }
        
        private void ClearDirectory()
        {
            // 删除包裹目录
            if (Directory.Exists(envPaths.buildDir))
            {
                Directory.Delete(envPaths.buildDir, true);
                Debug.Log($"Delete package root directory: {envPaths.buildDir}");
            }

            foreach (var buildTarget in buildTargets)
            {
                string platformOutputDir = envPaths.GetPlatformBuildDir(buildTarget);
                Directory.CreateDirectory(platformOutputDir);
            }
        }

        public void Build()
        {
            ClearDirectory();
            var config = envPaths.config;
            if (config.IsError())
            {
                Debug.LogError("build fail: config.txt is error");
                return;
            }

            PrePostAndBuild((bundleInfos) =>
            {
                var miniManifest = MiniProjectManifest.FromJson(config.ToJson());
                miniManifest.bundles = bundleInfos;
                miniManifest.kind = serializeKind;
                File.WriteAllBytes(envPaths.finalManifest, miniManifest.ToJson());
                EditorUtility.RevealInFinder(envPaths.buildDir);
            });
        }
    }
}