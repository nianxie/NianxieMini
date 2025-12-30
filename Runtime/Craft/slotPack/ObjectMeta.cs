using UnityEngine;

namespace Nianxie.Craft
{
    public abstract class ObjectMeta
    {
        public class SpriteMeta: ObjectMeta
        {
            public IntRectangle rect;
            public Vector2Int pivot;
            public float pixelsPerUnit;
        }
    }
}