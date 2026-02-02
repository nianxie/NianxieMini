
using UnityEngine;
using Nianxie.Riff;

namespace Nianxie.Craft
{
    public class SpriteJson:AbstractSlotJson<Sprite>
    {
        public string builtinPath;
        public int riffIndex;
        public SpriteMeta meta;
        public override Sprite Export(UnpackContext unpackContext)
        {
            var usage = unpackContext.GetTextureUsage(builtinPath, riffIndex);
            return usage.UseAndCreateSprite(meta).sprite;
        }
    }
}
