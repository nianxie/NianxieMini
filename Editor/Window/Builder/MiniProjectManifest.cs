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

        public MiniProjectManifest(MiniProjectConfig projConfig):base(projConfig)
        {
        }
    }
}

