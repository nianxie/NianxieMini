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

        public Vector2Int atlasSize;
        public SpriteMeta[] sprites;
        public BinaryMeta[] binaries;
    }
}