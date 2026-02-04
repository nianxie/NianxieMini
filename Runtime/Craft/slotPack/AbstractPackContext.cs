using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Nianxie.Craft
{
    public abstract class AbstractPackContext
    {
        protected virtual UniTask<byte[]> PackAtlasWebp(IntRectangle[] atlasPackRectArr, Vector2Int atlasSize)
        {
            throw new System.NotImplementedException();
        }
    }
}
