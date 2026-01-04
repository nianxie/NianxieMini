using System;
using Nianxie.Craft;
using UnityEngine;

namespace Nianxie.Riff
{
    public class ManifestJson:AbstractRiffJson
    {
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
        
        private static JsonCodec<ManifestJson> jsonCodec = new();
        public override string Dump()
        {
            return jsonCodec.Serialize(this);
        }
        public static ManifestJson Load(string data)
        {
            return jsonCodec.Deserialize(data);
        }
    }
}