using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLua;
using XLua.LuaDLL;

namespace Nianxie.Framework {
    public abstract class AsyncHelper : AbstractGameHelper
    {
        public void Call(LuaFunction fn)
        {
            fn.Action();
        }

        [HintReturn("Fn($self, Integer)")]
        public LuaFunction Sleep => gameManager.reflectEnv.boot.Sleep;
        
        [HintReturn("$function.nocheck@<_, T>(module:$self, fn:Fn():Ret(T)):Ret(Future(T)) end")]
        public lua_CSFunction Future => (IntPtr stackL) =>
        {
            var env = reflectEnv;
            if (Lua.lua_type(stackL, 2) != LuaTypes.LUA_TFUNCTION)
            {
                return Lua.luaL_error(stackL, "Future(fn) : second arg expect function ");
            }
            env.boot.NewFuture.push(stackL);
            Lua.lua_pushvalue(stackL, 2);
            var err = Lua.lua_pcall(stackL, 1, 1, 0);
            if (err != 0)
            {
                var errMsg = Lua.lua_tostring(stackL, -1);
                return Lua.luaL_error(stackL, $"Future create failed {errMsg}");
            }
            return 1;
        };
        
        [HintReturn("Fn($self, Integer):Ret(Future(Nil))")]
        public lua_CSFunction FutureSleep => reflectEnv.AsyncAction(this, async (int ms) =>
        {
            await UniTask.Delay(ms);
        });
    }
}
