using System;
using Nianxie.Craft;
using UnityEngine;

namespace Nianxie.Riff
{
    /// <summary>
    /// 用来存储riff package中的sprite和二进制对象的meta信息。
    /// </summary>
    public class ManifestRiffJson:AbstractRiffJson
    {
        public override string version => "0.0.1";

        public class RegionMeta
        {
            public IntRectangle rect;
        }

        public class BinaryMeta
        {
            public string ext;
        }

        public RegionMeta[] regions;
        public BinaryMeta[] binaries = { };
    }
}