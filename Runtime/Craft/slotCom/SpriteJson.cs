
using UnityEngine;

namespace Nianxie.Craft
{
    public class SpriteJson:AbstractSlotJson<Sprite>
    {
        public string defaultPath;
        public int sprite;
        public override Sprite Export(UnpackContext unpackContext)
        {
            if (string.IsNullOrEmpty(defaultPath))
            {
                return unpackContext.GetSprite(sprite);
            }
            else
            {
                return unpackContext.GetDefault(defaultPath) as Sprite;
            }
        }
    }
}
