using UnityEngine;

namespace Nianxie.Craft
{
    public class DefaultPackContext:IPackContext
    {
        int IPackContext.PutSprite(Sprite sprite)
        {
            throw new System.NotImplementedException("default pack can't take sprite");
        }

        int IPackContext.PutBinary(string ext, byte[] binary)
        {
            throw new System.NotImplementedException("default pack can't take binary");
        }
    }
}