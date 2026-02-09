using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [Serializable]
    public class SpritePolygon
    {
        [Serializable]
        public class Path
        {
            public Vector2[] points;
        }
        public List<Path> paths = new();
        public void ApplyTo(PolygonCollider2D collider2D)
        {
            collider2D.pathCount = paths.Count;
            for (int i = 0; i < paths.Count; i++)
            {
                collider2D.SetPath(i, paths[i].points);
            }
        }

        public static SpritePolygon FromPaths(List<Vector2[]> paths)
        {
            return new SpritePolygon
            {
                paths=paths.Select(points=>new SpritePolygon.Path
                {
                    points=points,
                }).ToList(),
            };
        }

        public List<Vector2[]> ToPaths()
        {
            return paths.Select(path=>path.points).ToList();
        }
    }

    public class PolygonJson:AbstractSlotJson<SpritePolygon>
    {
        public List<Vector2[]> paths;
        public override SpritePolygon Export(AssetUsageCenter usageCenter)
        {
            return SpritePolygon.FromPaths(paths);
        }
    }
}