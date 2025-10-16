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
    public class MiniProjectManifest: MiniProjectConfig
    {
        public BundleInfo[] bundles;
        public new static MiniProjectManifest FromJson(byte[] jsonBytes)
        {
            var jsonStr = Encoding.UTF8.GetString(jsonBytes);
            return JsonUtility.FromJson<MiniProjectManifest>(jsonStr);
        }
    }
}

