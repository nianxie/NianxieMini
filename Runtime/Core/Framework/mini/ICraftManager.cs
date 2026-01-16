using Cysharp.Threading.Tasks;
using Nianxie.Riff;
using UnityEngine;
using XLua;

namespace Nianxie.Framework
{
    public abstract class ICraftManager:MonoBehaviour
    {
        public abstract UniTask<LuaTable> PlayCraftTable(RiffPackage riffPackage);
    }
}