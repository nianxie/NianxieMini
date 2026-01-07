using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nianxie.Framework
{
    public abstract class AbstractCraftEntry:AbstractGameModule
    {
        public abstract void PlayEnding();
        public abstract UniTask PlayMain(MiniPlayArgs args);
        public abstract UniTask EditMain(MiniEditArgs args);
    }
}