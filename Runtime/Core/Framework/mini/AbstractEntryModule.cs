using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nianxie.Framework
{
    public abstract class AbstractEntryModule:AbstractGameModule
    {

        protected MiniBridge bridge;
        protected Func<UniTask> playFn;
        public void PreInit(MiniBridge _bridge, Func<UniTask> _playFn)
        {
            bridge = _bridge;
            playFn = _playFn;
        }

        public abstract void PlayEnding();
        public abstract UniTask PlayMain(MiniPlayArgs args);
        public abstract UniTask EditMain(MiniEditArgs args);
    }
}