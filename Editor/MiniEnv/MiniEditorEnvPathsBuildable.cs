using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nianxie.Editor
{
    public partial class MiniEditorEnvPaths
    {
        private const string BUNDLE_EXT = "bundle";
        
        private string GetPlatformFinalBundle(BuildTarget buildTarget)
        {
            return $"{buildDir}/{folder}_{buildTarget}.{BUNDLE_EXT}";
        }

        private string GetPlatformBuildDir(BuildTarget buildTarget)
        {
            return $"{buildDir}/{buildTarget}";
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
        
        private void ClearBuildDirectory()
        {
            // 删除包裹目录
            if (Directory.Exists(buildDir))
            {
                Directory.Delete(buildDir, true);
                Debug.Log($"Delete package root directory: {buildDir}");
            }

            foreach (var buildTarget in BuildTargets)
            {
                string platformOutputDir = GetPlatformBuildDir(buildTarget);
                Directory.CreateDirectory(platformOutputDir);
            }
        }

        public void Build()
        {
            ClearBuildDirectory();
            if (config.IsError())
            {
                Debug.LogError("build fail: config.txt is error");
                return;
            }

            // 收集非script资源
            var notScriptDict = CollectNotScript.Collect(reflectEnv);
            // 非script资源中显式引用的资源
            var explicitCollects = notScriptDict.Values.Where(a => a.isExplicit).ToArray();

            /*
             * asset bundle中包含三种资源：
             * 1. script 资源
             * 2. 显式引用的资源
             * 3. config.txt
             */
            var bundleBuild = new AssetBundleBuild()
            {
                assetBundleName = miniId,
                assetBundleVariant = "",
                assetNames = reflectEnv.scriptAssetDict.Keys
                    .Concat(explicitCollects.Select(a => a.path))
                    .Concat(new []{miniProjectConfig}).ToArray()
            };
            
            foreach (var buildTarget in BuildTargets)
            {
                string platformOutputDirectory = GetPlatformBuildDir(buildTarget);
                var buildOptions = GetBundleBuildOptions();
                AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(platformOutputDirectory, new []{bundleBuild}, buildOptions, buildTarget);
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
            var bundleInfos = new List<BundleInfo>(10);
            foreach (var buildTarget in BuildTargets)
            {
                string platformOutputDir = GetPlatformBuildDir(buildTarget);
                string srcMainName = $"{platformOutputDir}/{bundleBuild.assetBundleName}";
                string dstMainName = GetPlatformFinalBundle(buildTarget);
                File.Copy(srcMainName, dstMainName);
                if (BuildPipeline.GetCRCForAssetBundle(srcMainName, out var crc))
                {
                    bundleInfos.Add(new BundleInfo
                    {
                        name = dstMainName,
                        crc = crc,
                        size = new FileInfo(dstMainName).Length,
                    });
                }
                else
                {
                    throw new Exception($"Get crc failed for {srcMainName}");
                }
            }
            var miniManifest = MiniProjectManifest.FromJson(config.ToJson());
            miniManifest.bundles = bundleInfos.ToArray();
            File.WriteAllBytes(finalManifest, miniManifest.ToJson());
            EditorUtility.RevealInFinder(buildDir);
        }
    }
}