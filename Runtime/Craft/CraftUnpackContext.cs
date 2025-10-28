using UnityEngine;

namespace Nianxie.Craft
{
    public class CraftUnpackContext
    {
        private CraftJson craftJson;
        private Texture2D atlasTex;
        private bool finished = false;

        public CraftUnpackContext(CraftJson craftJson, Texture2D atlasTex)
        {
            this.craftJson = craftJson;
            this.atlasTex = atlasTex;
        }

        public void UnpackRoot(BehavSlot rootBehavSlot)
        {
            UnityEngine.Assertions.Assert.IsFalse(finished, "unpack context is finished");
            rootBehavSlot.UnpackFromJson(this, craftJson.root);
            finished = true;
        }

        public Sprite GenSprite(int spriteIndex, Vector2 pivot, float pixelsPerUnit)
        {
            var intRect = craftJson.atlasRects[spriteIndex];
            return Sprite.Create(atlasTex, intRect.ToUnityRect(), pivot, pixelsPerUnit);
        }

        public IntRectangle GetAtlasRect(int spriteIndex)
        {
            return craftJson.atlasRects[spriteIndex];
        }
    }
}
