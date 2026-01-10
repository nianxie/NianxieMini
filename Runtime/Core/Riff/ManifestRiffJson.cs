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

        public class SpriteMeta
        {
            public IntRectangle rect;
            public Vector2Int pivot;
            public float pixelsPerUnit;
        }

        public class BinaryMeta
        {
            public string ext;
        }

        public SpriteMeta[] sprites = { };
        public BinaryMeta[] binaries = { };
    }
}