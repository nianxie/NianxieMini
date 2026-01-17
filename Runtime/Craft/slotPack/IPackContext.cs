using UnityEngine;

namespace Nianxie.Craft
{
    public interface IPackContext
    {
        int PutSprite(Sprite sprite);
        int PutBinary(string ext, byte[] binary);
    }
}