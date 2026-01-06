using UnityEngine;

namespace Nianxie.Craft
{
    public interface IPutAsset
    {
        int PutSprite(Sprite sprite);
        int PutBinary(string ext, byte[] binary);
    }
}