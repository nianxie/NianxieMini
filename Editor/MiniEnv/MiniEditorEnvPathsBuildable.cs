using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Cysharp.Threading.Tasks;
using Nianxie.Preview;
using Nianxie.Utils;
using UnityEditor;
using UnityEngine;

namespace Nianxie.Editor
{
    public partial class MiniEditorEnvPaths
    {
        private static string AB_MAGIC = "_magic_20251015_";
        
        private string GetPlatformBuildDir(BuildTarget buildTarget)
        {
            return $"{buildDir}/{buildTarget}";
        }
        
        private string magicDir => $"{buildDir}/Magic";
        
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
            Directory.CreateDirectory(magicDir);
        }

        public void Build(BuildTarget[] targets)
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
             * asset bundle的构建中显示包含三种资源：
             * 1. script 资源
             * 2. prefab和script引用的资源
             * 3. config.txt
             */
            var bundleBuild = new AssetBundleBuild()
            {
                assetBundleName = $"{AB_MAGIC}{Guid.NewGuid():N}", 
                assetBundleVariant = "",
                assetNames = reflectEnv.scriptAssetDict.Keys
                    .Concat(explicitCollects.Select(a => a.path))
                    .Concat(new []{miniProjectConfig}).ToArray()
            };
            
            foreach (var buildTarget in targets)
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
            foreach (var buildTarget in targets)
            {
                string platformOutputDir = GetPlatformBuildDir(buildTarget);
                string srcMainName = $"{platformOutputDir}/{bundleBuild.assetBundleName}";
                string dstMainName = finalBundleDict[buildTarget];
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
            var miniManifest = new MiniBundleManifest(bundleInfos.ToArray(), config);
            File.WriteAllBytes(finalManifest, miniManifest.ToJson());
            EditorUtility.RevealInFinder(finalManifest);
        }
        
        // 使用一些取巧的手段替换AssetBundle的name，必须是uncompress格式的。
        private static void RenameMagicBundle(byte[] bundleBytes, string targetGuid)
        {
            string bundleName = null;
            {
                var bundle = AssetBundle.LoadFromMemory(bundleBytes);
                bundleName = bundle.name;
                bundle.Unload(true);
            }
            var targetNameBytes = Encoding.ASCII.GetBytes($"{AB_MAGIC}{targetGuid}");
            if (!bundleName.StartsWith(AB_MAGIC) || bundleName.Length != targetNameBytes.Length)
            {
                throw new Exception($"bundle is not a valid mini bundle, name={bundleName}");
            }
            // 1. 匹配
            var bundleNameBytes = Encoding.ASCII.GetBytes(bundleName);
            var bundleSpan = new ReadOnlySpan<byte>(bundleBytes);
            var matchCount = 0;
            var matchNamePosArr = new int[2]{0, 0};
            for (int i = 0; i < bundleBytes.Length - bundleNameBytes.Length; i++)
            {
                var nameSpan = bundleSpan.Slice(i, bundleNameBytes.Length);
                if (nameSpan.SequenceCompareTo(bundleNameBytes) == 0)
                {
                    if (matchCount >= 2)
                    {
                        throw new Exception($"bundle is not a valid mini bundle, name match more than 2");
                    }
                    matchNamePosArr[matchCount++] = i;
                }
            }
            if (matchCount != 2)
            {
                throw new Exception($"bundle is not a valid mini bundle, name match less than 2");
            }
            // 2. 替换
            var name1Pos = matchNamePosArr[0];
            var name2Pos = matchNamePosArr[1];
            for (int i = 0; i < targetNameBytes.Length; i++)
            {
                bundleBytes[name1Pos + i] = targetNameBytes[i];
                bundleBytes[name2Pos + i] = targetNameBytes[i];
            }
        }
        
        /// <summary>
        /// 为了避免AssetBundle的name冲突，根据服务器返回的guid对AssetBundle进行重命名
        /// </summary>
        /// <param name="targetGuid">从服务器获取的guid</param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public async UniTask<string> ExecuteRename(string targetGuid, BuildTarget platform)
        {
            UnityEngine.Assertions.Assert.IsTrue(targetGuid.Length == 32);
            AssetBundle.UnloadAllAssetBundles(true);
            var originPath = finalBundleDict[platform];
            // 1. 解压
            var targetPath = $"{magicDir}/{targetGuid}_{platform}.{NianxieConst.Ext.BUNDLE}";
            await AssetBundle.RecompressAssetBundleAsync(originPath, targetPath, BuildCompression.Uncompressed, 0, ThreadPriority.High).ToUniTask();
            var bundleBytes = await File.ReadAllBytesAsync(targetPath);
            await File.WriteAllBytesAsync(targetPath, bundleBytes);
            // 2. magic重命名
            RenameMagicBundle(bundleBytes, targetGuid);
            // 3. 压缩
            await AssetBundle.RecompressAssetBundleAsync(targetPath, targetPath, BuildCompression.LZ4Runtime, 0, ThreadPriority.High).ToUniTask();
            return targetPath;
        }
    }
}