using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace Nianxie.Framework
{
    [BlackList]
    public abstract class AbstractCraftEntry:AbstractGameModule
    {
        public abstract LuaTable PlayBuild();
    }
}