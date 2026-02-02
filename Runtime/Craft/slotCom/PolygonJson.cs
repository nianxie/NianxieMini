using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [Serializable]
    public class SpritePolygon
    {
        private List<Vector2[]> paths;

        public SpritePolygon(List<Vector2[]> paths)
        {
            this.paths = paths;
        }

        public void ApplyTo(PolygonCollider2D collider2D)
        {
            collider2D.pathCount = paths.Count;
            for (int i = 0; i < paths.Count; i++)
            {
                collider2D.SetPath(i, paths[i]);
            }
        }
    }

    public class PolygonJson:AbstractSlotJson<SpritePolygon>
    {
        public List<Vector2[]> paths;
        public override SpritePolygon Export(UnpackContext unpackContext)
        {
            return new SpritePolygon(paths);
        }
    }
}