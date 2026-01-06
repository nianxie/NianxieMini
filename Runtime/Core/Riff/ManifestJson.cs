using System;
using Nianxie.Craft;
using UnityEngine;

namespace Nianxie.Riff
{
    public class ManifestJson:AbstractRiffJson
    {
        public override string kind => nameof(ManifestJson);
        public override string version => "0.0.1";

        static ManifestJson()
        {
            JsonCodec.RegisterFactory<ManifestJson>();
        }

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