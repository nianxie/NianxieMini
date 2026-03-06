using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Components
{
    public abstract class SpawnBehaviour: MonoBehaviour
    {
        // 需要让带有SpawnBehaviour的Luafab在Instantiate的时候持有luaModule的引用，由AssetModule遍历赋值
        [NonEditable]
        [BlackList] public AbstractGameManager gameManager;
    }
}