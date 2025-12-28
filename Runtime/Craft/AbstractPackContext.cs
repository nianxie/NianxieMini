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
        protected bool finished = false;
        protected class AtlasSprite
        {
            public IntRectangle atlasRect;
            public Sprite sprite;
        }
        public int AddSprite(Sprite sprite)
        {
            var index = spriteList.Count;
            var rect = sprite.rect;
            spriteList.Add(new AtlasSprite
            {
                atlasRect = new IntRectangle(0, 0, (int)rect.width, (int)rect.height),
                sprite = sprite,
            });
            return index;
        }
        public void PackRoot(SlotBehaviour craftSlot)
        {
            UnityEngine.Assertions.Assert.IsFalse(finished, "pack context is finished");
            craftJson.root = (SlotBehavJson)craftSlot.PackToJson(this);
            var sortedRectArr = spriteList.Select(s => s.atlasRect).OrderByDescending(r => r.width * r.height).ToArray();
            craftJson.atlasSize = RectanglePacker.PackRectsInplace(sortedRectArr);
            craftJson.atlasRects = spriteList.Select(a => a.atlasRect).ToArray();
            finished = true;
        }
    }

}
