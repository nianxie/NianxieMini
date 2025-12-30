using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Nianxie.Craft
{

    public class AbstractUnpackContext:IGetAsset
    {
        protected Sprite[] spriteList;

        UnityEngine.Sprite IGetAsset.GetSprite(int spriteIndex)
        {
            return spriteList[spriteIndex];
        }

    }
}
