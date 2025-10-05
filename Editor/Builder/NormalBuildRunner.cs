using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor;

namespace Nianxie.Editor
{
    public class NormalBuildRunner:AbstractBuildRunner
    {
        public NormalBuildRunner(MiniEditorEnvPaths envPaths, BuildTarget[] buildTargets) : base(envPaths, buildTargets)
        {
        }

        protected override string serializeKind => MiniEditorEnvPaths.BUNDLE_EXT;

        protected override void PrePostAndBuild(Action<BundleInfo[]> onSucc)
        {
            // TODO share bundle使用更统一的方式处理
            var notScriptDict = CollectNotScript.Collect(envPaths.reflectEnv);

            // main bundle，包含script
            var explicitCollects = notScriptDict.Values.Where(a => a.isExplicit);
            mainBundle.assetNames = envPaths.reflectEnv.scriptAssetDict.Keys
                .Concat(new []{envPaths.miniProjectConfig})
                .Concat(explicitCollects.Select(a => a.path)).ToArray();

            // share bundle，字体共享bundle
            PipelineBuild(new []{mainBundle});
            var bundleInfos = new List<BundleInfo>(10);
            foreach (var buildTarget in buildTargets)
            {
                string platformOutputDir = envPaths.GetPlatformBuildDir(buildTarget);
                string srcMainName = $"{platformOutputDir}/{mainBundle.assetBundleName}";
                string dstMainName = envPaths.GetFinalBundlePath(buildTarget);
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
            onSucc(bundleInfos.ToArray());
        }
    }
}