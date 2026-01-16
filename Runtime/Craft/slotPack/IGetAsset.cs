using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public interface IGetAsset
    {
        LuaTable NewTable();
        Sprite GetSprite(int spriteIndex);
    }
}