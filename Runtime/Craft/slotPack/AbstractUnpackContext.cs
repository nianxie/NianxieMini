using Nianxie.Riff;

namespace Nianxie.Craft
{

    public abstract class AbstractUnpackContext:IGetAsset
    {
        protected abstract RiffPackage package { get; }

        UnityEngine.Sprite IGetAsset.GetSprite(int spriteIndex)
        {
            return package.sprites[spriteIndex];
        }

    }
}
