
using UnityEngine;
using Nianxie.Riff;

namespace Nianxie.Craft
{
    public class SpriteJson:AbstractSlotJson<Sprite>, IUsageLocator
    {
        public string builtinPath;
        public int riffIndex;
        public SpriteMeta meta;
        public override Sprite Export(AssetUsageCenter usageCenter)
        {
            var usage = usageCenter.textureUsagePool.FindUsage(this);
            return usage.UseAndCreateSprite(meta).sprite;
        }

        string IUsageLocator.GetBuiltinPath()
        {
            return builtinPath;
        }

        int IUsageLocator.GetRiffIndex()
        {
            return riffIndex;
        }
    }
}
