using System;
using System.Text;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;

namespace Nianxie.Preview
{
    [Serializable]
    public class BundleInfo
    {
        public string name;
        public uint crc;
        public long size;
    }
    [Serializable]
    public class MiniBundleManifest
    {
        public BundleInfo[] bundles;
        public MiniProjectConfig config;

        public MiniBundleManifest(BundleInfo[] bundles, MiniProjectConfig config)
        {
            this.config = config;
            this.bundles = bundles;
        }
        
        public byte[] ToJson()
        {
            var jsonStr = JsonUtility.ToJson(this, true);
            return Encoding.UTF8.GetBytes(jsonStr);
        }
        public static MiniBundleManifest FromJson(byte[] jsonBytes)
        {
            var jsonStr = Encoding.UTF8.GetString(jsonBytes);
            return JsonUtility.FromJson<MiniBundleManifest>(jsonStr);
        }

        public static string GetFinalBuildDir(string folder)
        {
            return $"{NianxieConst.MiniBundlesOutput}/{folder}";
        }

        public static string GetFinalBundlePath(string folder, string buildTarget)
        {
            return $"{GetFinalBuildDir(folder)}/{folder}_{buildTarget}.{NianxieConst.Ext.BUNDLE}";
        }
        public static string GetFinalManifestPath(string folder)
        {
            return $"{GetFinalBuildDir(folder)}/{folder}.json";
        }
    }
}