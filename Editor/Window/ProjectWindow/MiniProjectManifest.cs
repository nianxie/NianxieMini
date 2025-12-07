using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Nianxie.Framework;
using UnityEngine;

namespace Nianxie.Editor
{
    [Serializable]
    public class BundleInfo
    {
        public string name;
        public uint crc;
        public long size;
    }
    [Serializable]
    public class MiniProjectManifest
    {
        public BundleInfo[] bundles;
        public MiniProjectConfig config;

        public MiniProjectManifest(BundleInfo[] bundles, MiniProjectConfig config)
        {
            this.config = config;
            this.bundles = bundles;
        }
        
        public byte[] ToJson()
        {
            var jsonStr = JsonUtility.ToJson(this, true);
            return Encoding.UTF8.GetBytes(jsonStr);
        }
    }
}

