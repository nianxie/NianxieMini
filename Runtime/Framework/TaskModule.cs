using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLua;

namespace Nianxie.Framework {
    // TODO TaskModule 这种实现不好，考虑后续重构掉
    public abstract class TaskModule : AbstractGameHelper
    {

        [HintReturn("$function.nocheck@<_, T>(module:$self, fn:Fn():Ret(T)):Ret(LuaTask(T)) end")]
        public LuaFunction runTask => gameManager.reflectEnv.bootTask;
        [HintReturn("Fn($self, Integer)")]
        public LuaFunction sleep => gameManager.reflectEnv.bootSleep;
        
        public void setTimeout(int ms, LuaFunction fn)
        {
            if (ms <= 0)
            {
                try
                {
                    fn.Action();
                    fn.Dispose();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"exception when setTimeout {e}");
                }
            }
            else
            {
                UniTask.Create(async () =>
                {
                    await UniTask.Delay(ms);
                    fn.Action();
                    fn.Dispose();
                }).Forget();
            }
        }
    }
}
