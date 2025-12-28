using System.IO;
using Nianxie.Framework;
using Nianxie.Utils;

namespace Nianxie.Preview
{
    public class PreviewMiniInfo
    {
        public class BundleInfo
        {
            public string iosBundle;
            public string androidBundle;
            public string webglBundle;
            public MiniProjectConfig config;

            public BundleInfo(string folder, MiniBundleManifest manifest)
            {
                iosBundle = MiniBundleManifest.GetFinalBundlePath(folder, "iOS");
                androidBundle = MiniBundleManifest.GetFinalBundlePath(folder, "Android");
                webglBundle = MiniBundleManifest.GetFinalBundlePath(folder, "WebGL");
                config = manifest.config;
            }
        }

        public string folder { get; }
        public MiniProjectConfig config { get; }
        public BundleInfo bundleInfo { get; }

        public PreviewMiniInfo(string folder)
        {
            this.folder = folder;
            var manifestFile = MiniBundleManifest.GetFinalManifestPath(folder);
            if (File.Exists(manifestFile))
            {
                var manifest = MiniBundleManifest.FromJson(File.ReadAllBytes(manifestFile));
                bundleInfo = new BundleInfo(folder, manifest);
            }
            var configFile = $"{NianxieConst.MiniPrefixPath}/{folder}/{NianxieConst.ConfigTxt}";
            var configBytes = File.ReadAllBytes(configFile);
            config = MiniProjectConfig.FromJson(configBytes);
        }
    }
}