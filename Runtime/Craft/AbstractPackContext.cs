using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Nianxie.Craft
{
    public abstract class AbstractPackContext
    {
        protected CraftJson craftJson = new();
        protected List<AtlasSprite> spriteList = new();
        private bool finished = false;
        protected class AtlasSprite
        {
            public IntRectangle atlasRect;
            public IntRectangle cropRect;
            public Texture2D sourceTex;
        }
        public int AddSprite(Texture2D source, IntRectangle cropRect, Vector2Int packSize)
        {
            var index = spriteList.Count;
            spriteList.Add(new AtlasSprite
            {
                atlasRect = new IntRectangle(0,0, packSize.x, packSize.y),
                cropRect = cropRect,
                sourceTex = source,
            });
            return index;
        }
        public void PackRoot(SlotBehaviour craftSlot)
        {
            UnityEngine.Assertions.Assert.IsFalse(finished, "pack context is finished");
            craftJson.root = craftSlot.PackToJson(this);
            var sortedRectArr = spriteList.Select(s => s.atlasRect).OrderByDescending(r => r.width * r.height).ToArray();
            craftJson.atlasSize = RectanglePacker.PackRectsInplace(sortedRectArr);
            craftJson.atlasRects = spriteList.Select(a => a.atlasRect).ToArray();
            finished = true;
        }
    }

}
