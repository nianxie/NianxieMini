
using UnityEngine;

namespace Nianxie.Craft
{
    public class SpriteJson:AbstractSlotJson<Sprite>
    {
        public int sprite;
        public override Sprite Export(IGetAsset getAsset)
        {
            return getAsset.GetSprite(sprite);
        }
    }
}
